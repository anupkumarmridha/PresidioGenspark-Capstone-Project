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
    public class ReactionController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<ReactionController> _logger;
        private readonly IReactionService _reactionService;
        private readonly string _reactionsTopic;


        public ReactionController(ICacheService cacheService,IReactionService reactionService, ILogger<ReactionController> logger)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _reactionService = reactionService ?? throw new ArgumentNullException(nameof(reactionService));
        }

        [Authorize]
        [HttpPost]
        [Route("add")]

        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddReactionAsync([FromBody] ReactionDto reactionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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

                await _reactionService.AddReactionAsync(reaction);

                // Cache the reaction
                var cacheKey = $"reaction-{userId}-{reactionDto.ArticleId}";
                await _cacheService.SetAsync(cacheKey, reaction, TimeSpan.FromHours(1));
                _logger.LogInformation($"Reaction added and cached: ArticleId={reactionDto.ArticleId}, UserId={userId}");
                return Ok("Reaction added successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding reaction.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while adding reaction.");
            }
        }

        [HttpGet]
        [Route("get")]
        [ProducesResponseType(typeof(IEnumerable<ArticleReactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetAllReactionByArtilceAsync([FromQuery] string articleId)
        {
            if (string.IsNullOrEmpty(articleId))
            {
                _logger.LogWarning("GetReactionAsync: ArticleId is null or empty.");
                return BadRequest("ArticleId is required.");
            }

            try
            {
                var reactions = await _reactionService.GetAllReactionsByArticleAsync(articleId);
                _logger.LogInformation($"Reactions retrieved from database and cached: ArticleId={articleId}");
                return Ok(reactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving reaction.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while retrieving reaction.");
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("remove")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
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

                await _reactionService.RemoveReactionAsync(userId, articleId);
                    // Remove from cache
                    var cacheKey = $"reaction-{userId}-{articleId}";
                    await _cacheService.RemoveAsync(cacheKey);

                    _logger.LogInformation($"Reaction removed and cache cleared: ArticleId={articleId}, UserId={userId}");
                    return Ok("Reaction removed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing reaction.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while removing reaction.");
            }
        }

        [Authorize]
        [HttpPut]
        [Route("update")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateReactionAsync([FromBody] ReactionDto reactionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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

                await _reactionService.UpdateReactionAsync(reaction);

                    var cacheKey = $"reaction-{userId}-{reactionDto.ArticleId}";
                    await _cacheService.SetAsync(cacheKey, reaction, TimeSpan.FromHours(1));

                    _logger.LogInformation($"Reaction updated and cached: ArticleId={reactionDto.ArticleId}, UserId={userId}");
                    return Ok("Reaction updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating reaction.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while updating reaction.");
            }
        }
    }
}
