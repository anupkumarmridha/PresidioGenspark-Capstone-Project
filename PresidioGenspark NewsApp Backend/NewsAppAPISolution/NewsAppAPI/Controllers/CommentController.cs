using Confluent.Kafka;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NewsAppAPI.Cache;
using NewsAppAPI.DTOs;
using NewsAppAPI.Kafka.Producers;
using NewsAppAPI.Models;
using NewsAppAPI.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NewsAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CommentController> _logger;
        private readonly string _commentsTopic;

        public CommentController(
            ICommentService commentService,
            IKafkaProducer kafkaProducer,
            ICacheService cacheService,
            ILogger<CommentController> logger,
            IConfiguration configuration)
        {
            _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
            _kafkaProducer = kafkaProducer ?? throw new ArgumentNullException(nameof(kafkaProducer));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commentsTopic = configuration["Kafka:CommentsTopic"];
        }

        [HttpGet]
        [Route("article/{articleId}")]
        public async Task<IActionResult> GetCommentsByArticleIdAsync(string articleId)
        {
            if (string.IsNullOrEmpty(articleId))
            {
                _logger.LogWarning("GetCommentsByArticleIdAsync: ArticleId is null or empty.");
                return BadRequest("ArticleId is required.");
            }

            try
            {
                var cacheKey = $"comments_{articleId}";
                var cachedComments = await _cacheService.GetAsync(cacheKey);

                if (cachedComments != null)
                {
                    _logger.LogInformation($"Retrieved comments from cache for articleId: {articleId}");
                    return Ok(cachedComments);
                }

                var comments = await _commentService.GetCommentsByArticleIdAsync(articleId);

                if (comments.Any())
                {
                    await _cacheService.SetAsync(cacheKey, comments, TimeSpan.FromMinutes(30)); // Cache for 30 minutes
                }

                _logger.LogInformation($"Retrieved {comments.Count()} comments for articleId: {articleId}");
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving comments.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while retrieving comments.");
            }
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetCommentByIdAsync(int id)
        {
            try
            {
                var cacheKey = $"comment_{id}";
                var cachedComment = await _cacheService.GetAsync(cacheKey);

                if (cachedComment != null)
                {
                    _logger.LogInformation($"Retrieved comment from cache with id: {id}");
                    return Ok(cachedComment);
                }

                var comment = await _commentService.GetCommentByIdAsync(id);

                if (comment == null)
                {
                    _logger.LogInformation($"No comment found with id: {id}");
                    return NotFound("Comment not found.");
                }

                await _cacheService.SetAsync(cacheKey, comment, TimeSpan.FromMinutes(30)); // Cache for 30 minutes
                _logger.LogInformation($"Retrieved comment with id: {id}");
                return Ok(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving comment.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while retrieving comment.");
            }
        }

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddCommentAsync([FromBody] CommentFEDto commentDto)
        {
            if (commentDto == null)
            {
                _logger.LogWarning("AddCommentAsync: CommentDto is null.");
                return BadRequest("CommentDto is null.");
            }

            try
            {
                var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.Name));
                var comment = new Comment
                {
                    ArticleId = commentDto.ArticleId,
                    Content = commentDto.Content,
                    UserId = userId,
                    ParentId = commentDto.ParentId,
                    CreatedAt = DateTime.UtcNow
                };

                // Send message to Kafka
                var kafkaMessageDto = new KafkaMessageDto
                {
                    Topic = _commentsTopic,
                    Message = JsonConvert.SerializeObject(comment),
                    Operation = "add"
                };
                var deliveryResult = await _kafkaProducer.ProduceAsync(kafkaMessageDto);

                if (deliveryResult.Status == PersistenceStatus.Persisted)
                {
                    var cacheKey = $"comments_{comment.ArticleId}";
                    await _cacheService.RemoveAsync(cacheKey); // Invalidate cache
                    _logger.LogInformation($"Comment added and cache invalidated: ArticleId={commentDto.ArticleId}, UserId={userId}");
                    return Ok("Comment added successfully.");
                }
                else
                {
                    _logger.LogError("Failed to add comment to Kafka.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to add comment.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding comment.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while adding comment.");
            }
        }

        [HttpPut]
        [Route("update/{id}")]
        public async Task<IActionResult> UpdateCommentAsync(int id, [FromBody] CommentFEDto commentDto)
        {
            if (commentDto == null)
            {
                _logger.LogWarning("UpdateCommentAsync: CommentDto is null.");
                return BadRequest("CommentDto is null.");
            }

            try
            {
                var comment = await _commentService.GetCommentByIdAsync(id);
                if (comment == null)
                {
                    _logger.LogInformation($"No comment found with id: {id}");
                    return NotFound("Comment not found.");
                }

                comment.Content = commentDto.Content;
                comment.ParentId = commentDto.ParentId;

                // Send message to Kafka
                var kafkaMessageDto = new KafkaMessageDto
                {
                    Topic = _commentsTopic,
                    Message = JsonConvert.SerializeObject(comment),
                    Operation = "update"
                };
                var deliveryResult = await _kafkaProducer.ProduceAsync(kafkaMessageDto);

                if (deliveryResult.Status == PersistenceStatus.Persisted)
                {
                    var cacheKey = $"comment_{id}";
                    await _cacheService.RemoveAsync(cacheKey); // Invalidate cache
                    _logger.LogInformation($"Comment updated and cache invalidated: Id={id}");
                    return Ok("Comment updated successfully.");
                }
                else
                {
                    _logger.LogError("Failed to update comment in Kafka.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update comment.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating comment.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while updating comment.");
            }
        }

        [HttpDelete]
        [Route("delete/{id}")]
        public async Task<IActionResult> DeleteCommentAsync(int id)
        {
            try
            {
                var comment = await _commentService.GetCommentByIdAsync(id);
                if (comment == null)
                {
                    _logger.LogInformation($"No comment found with id: {id}");
                    return NotFound("Comment not found.");
                }

                // Send message to Kafka
                var kafkaMessageDto = new KafkaMessageDto
                {
                    Topic = _commentsTopic,
                    Message = JsonConvert.SerializeObject(comment),
                    Operation = "delete"
                };
                var deliveryResult = await _kafkaProducer.ProduceAsync(kafkaMessageDto);

                if (deliveryResult.Status == PersistenceStatus.Persisted)
                {
                    var cacheKey = $"comment_{id}";
                    await _cacheService.RemoveAsync(cacheKey); // Invalidate cache
                    _logger.LogInformation($"Comment deleted and cache invalidated: Id={id}");
                    return Ok("Comment deleted successfully.");
                }
                else
                {
                    _logger.LogError("Failed to delete comment in Kafka.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete comment.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting comment.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while deleting comment.");
            }
        }
    }
}
