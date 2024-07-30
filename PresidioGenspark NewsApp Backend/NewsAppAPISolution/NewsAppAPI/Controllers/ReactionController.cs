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
using System.Security.Claims;
using System.Threading.Tasks;

namespace NewsAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReactionController : ControllerBase
    {
        private readonly IKafkaProducer _kafkaProducer;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ReactionController> _logger;
        private readonly IReactionService _reactionService;


        public ReactionController(IKafkaProducer kafkaProducer, ICacheService cacheService,IReactionService reactionService, ILogger<ReactionController> logger)
        {
            _kafkaProducer = kafkaProducer ?? throw new ArgumentNullException(nameof(kafkaProducer));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _reactionService = reactionService ?? throw new ArgumentNullException(nameof(reactionService));
        }

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddReactionAsync([FromBody] ReactionDto reactionDto)
        {
            if (reactionDto == null)
            {
                _logger.LogWarning("AddReactionAsync: ReactionDto is null.");
                return BadRequest("ReactionDto is null.");
            }

            try
            {
                var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.Name));
                var reaction = new Reaction
                {
                    ArticleId = reactionDto.ArticleId,
                    UserId = userId,
                    ReactionType = reactionDto.ReactionType
                };

                // Produce Kafka message
                var kafkaMessageDto = new KafkaMessageDto
                {
                    Topic = "reactions",
                    Message = JsonConvert.SerializeObject(reaction),
                    Operation = "add"
                };

                var deliveryResult = await _kafkaProducer.ProduceAsync(kafkaMessageDto);

                if (deliveryResult.Status == PersistenceStatus.Persisted)
                {
                    // Cache the reaction
                    var cacheKey = $"reaction-{userId}-{reactionDto.ArticleId}";
                    await _cacheService.SetAsync(cacheKey, reaction, TimeSpan.FromHours(1));

                    _logger.LogInformation($"Reaction added and cached: ArticleId={reactionDto.ArticleId}, UserId={userId}");
                    return Ok("Reaction added successfully.");
                }
                else
                {
                    _logger.LogWarning("AddReactionAsync: Kafka message was not persisted.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to add reaction.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding reaction.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while adding reaction.");
            }
        }

        [HttpGet]
        [Route("get")]
        public async Task<IActionResult> GetReactionAsync([FromQuery] string articleId)
        {
            if (string.IsNullOrEmpty(articleId))
            {
                _logger.LogWarning("GetReactionAsync: ArticleId is null or empty.");
                return BadRequest("ArticleId is required.");
            }

            try
            {
                var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.Name));

                // Check cache first
                var cacheKey = $"reaction-{userId}-{articleId}";
                var cachedReaction = await _cacheService.GetAsync(cacheKey);

                if (cachedReaction != null)
                {
                    _logger.LogInformation($"Reaction retrieved from cache: ArticleId={articleId}, UserId={userId}");
                    return Ok(cachedReaction);
                }

                var reaction = await _reactionService.GetReactionAsync(userId, articleId);

                if (reaction == null)
                {
                    _logger.LogInformation($"No reaction found for ArticleId={articleId}, UserId={userId}");
                    return NotFound("Reaction not found.");
                }

                // Cache the fetched reaction
                await _cacheService.SetAsync(cacheKey, reaction, TimeSpan.FromHours(1));

                _logger.LogInformation($"Reaction retrieved from database and cached: ArticleId={articleId}, UserId={userId}");
                return Ok(reaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving reaction.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while retrieving reaction.");
            }
        }

        [HttpDelete]
        [Route("remove")]
        public async Task<IActionResult> RemoveReactionAsync([FromQuery] string articleId)
        {
            if (string.IsNullOrEmpty(articleId))
            {
                _logger.LogWarning("RemoveReactionAsync: ArticleId is null or empty.");
                return BadRequest("ArticleId is required.");
            }

            try
            {
                var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.Name));

                // Produce Kafka message
                var kafkaMessageDto = new KafkaMessageDto
                {
                    Topic = "reactions",
                    Message = JsonConvert.SerializeObject(new { ArticleId = articleId, UserId = userId }),
                    Operation = "remove"
                };

                var deliveryResult = await _kafkaProducer.ProduceAsync(kafkaMessageDto);

                if (deliveryResult.Status == PersistenceStatus.Persisted)
                {
                    // Remove from cache
                    var cacheKey = $"reaction-{userId}-{articleId}";
                    await _cacheService.RemoveAsync(cacheKey);

                    _logger.LogInformation($"Reaction removed and cache cleared: ArticleId={articleId}, UserId={userId}");
                    return Ok("Reaction removed successfully.");
                }
                else
                {
                    _logger.LogWarning("RemoveReactionAsync: Kafka message was not persisted.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to remove reaction.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing reaction.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while removing reaction.");
            }
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> UpdateReactionAsync([FromBody] ReactionDto reactionDto)
        {
            if (reactionDto == null)
            {
                _logger.LogWarning("UpdateReactionAsync: ReactionDto is null.");
                return BadRequest("ReactionDto is null.");
            }

            try
            {
                var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.Name));
                var reaction = new Reaction
                {
                    ArticleId = reactionDto.ArticleId,
                    UserId = userId,
                    ReactionType = reactionDto.ReactionType
                };

                // Produce Kafka message
                var kafkaMessageDto = new KafkaMessageDto
                {
                    Topic = "reactions",
                    Message = JsonConvert.SerializeObject(reaction),
                    Operation = "update"
                };

                var deliveryResult = await _kafkaProducer.ProduceAsync(kafkaMessageDto);

                if (deliveryResult.Status == PersistenceStatus.Persisted)
                {
                    // Update cache
                    var cacheKey = $"reaction-{userId}-{reactionDto.ArticleId}";
                    await _cacheService.SetAsync(cacheKey, reaction, TimeSpan.FromHours(1));

                    _logger.LogInformation($"Reaction updated and cached: ArticleId={reactionDto.ArticleId}, UserId={userId}");
                    return Ok("Reaction updated successfully.");
                }
                else
                {
                    _logger.LogWarning("UpdateReactionAsync: Kafka message was not persisted.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update reaction.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating reaction.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while updating reaction.");
            }
        }
    }
}
