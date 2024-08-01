using Confluent.Kafka;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NewsAppAPI.Cache;
using NewsAppAPI.DTOs;
using NewsAppAPI.Exceptions;
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
    
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        private readonly ICacheService _cacheService;
        private readonly ILogger<CommentController> _logger;
  

        public CommentController(
            ICommentService commentService,
            ICacheService cacheService,
            ILogger<CommentController> logger
            )
        {
            _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
           
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    
        }

        [HttpGet]
        [Route("article/{articleId}")]
        [ProducesResponseType(typeof(IEnumerable<Comment>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCommentsByArticleIdAsync(string articleId)
        {
            if (string.IsNullOrEmpty(articleId))
            {
                _logger.LogWarning("GetCommentsByArticleIdAsync: ArticleId is null or empty.");
                return BadRequest(new ErrorModel(400, "ArticleId is required."));
            }

            try
            {
                var comments = await _commentService.GetCommentsByArticleIdAsync(articleId);

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
        [ProducesResponseType(typeof(Comment), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
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
                    return NotFound(new ErrorModel(404, "Comment Not Found"));
                }

                await _cacheService.SetAsync(cacheKey, comment, TimeSpan.FromMinutes(30)); // Cache for 30 minutes
                _logger.LogInformation($"Retrieved comment with id: {id}");
                return Ok(comment);
            }
            catch(NotFoundException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving comment.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while retrieving comment.");
            }
        }

        [Authorize]
        [HttpPost]
        [Route("add")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddCommentAsync([FromBody] CommentFEDto commentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogError("User ID claim not found or empty.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "User ID claim not found.");
                }

                if (!int.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogError($"User ID claim '{userIdClaim}' is not a valid integer.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Invalid user ID.");
                }
                var comment = new Comment
                {
                    ArticleId = commentDto.ArticleId,
                    Content = commentDto.Content,
                    UserId = userId,
                    ParentId = commentDto.ParentId,
                    CreatedAt = DateTime.UtcNow
                };

                await _commentService.AddCommentAsync(comment);
                    var cacheKey = $"comments_{comment.ArticleId}";
                    await _cacheService.RemoveAsync(cacheKey); // Invalidate cache
                    _logger.LogInformation($"Comment added and cache invalidated: ArticleId={commentDto.ArticleId}, UserId={userId}");
                    return Ok("Comment added successfully.");
             

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding comment.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while adding comment.");
            }
        }

        [Authorize]
        [HttpPut]
        [Route("update/{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCommentAsync(int id, [FromBody] CommentFEDto commentDto)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.Name));
                var comment = await _commentService.GetCommentByIdAsync(id);
               if(comment.UserId != userId)
                {
                    return Unauthorized(new ErrorModel(401, "You are not authorized to update this comment."));
                }

                comment.Content = commentDto.Content;
                comment.ParentId = commentDto.ParentId;

                // Send message to Kafka
                await _commentService.UpdateCommentAsync(comment);

                var cacheKey = $"comment_{id}";
                    await _cacheService.RemoveAsync(cacheKey); // Invalidate cache
                    _logger.LogInformation($"Comment updated and cache invalidated: Id={id}");
                    return Ok("Comment updated successfully.");


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating comment.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while updating comment.");
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("delete/{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCommentAsync(int id)
        {
            try
            {
                var comment = await _commentService.GetCommentByIdAsync(id);
                if (comment == null)
                {
                    _logger.LogInformation($"No comment found with id: {id}");
                    return NotFound(new ErrorModel(404, "Comment not found"));
                }
                await _commentService.DeleteCommentAsync(id);

                 var cacheKey = $"comment_{id}";
                    await _cacheService.RemoveAsync(cacheKey); // Invalidate cache
                    _logger.LogInformation($"Comment deleted and cache invalidated: Id={id}");
                    return Ok("Comment deleted successfully.");
            
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting comment.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while deleting comment.");
            }
        }
    }
}
