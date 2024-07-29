using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NewsAppAPI.DTOs;
using NewsAppAPI.Models;
using NewsAppAPI.Services.Interfaces;
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
        private readonly IReactionService _reactionService;
        private readonly ILogger<ReactionController> _logger;

        public ReactionController(IReactionService reactionService, ILogger<ReactionController> logger)
        {
            _reactionService = reactionService ?? throw new ArgumentNullException(nameof(reactionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

                await _reactionService.AddReactionAsync(reaction);
                _logger.LogInformation($"Reaction added: ArticleId={reactionDto.ArticleId}, UserId={userId}");
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
                var reaction = await _reactionService.GetReactionAsync(userId, articleId);

                if (reaction == null)
                {
                    _logger.LogInformation($"No reaction found for ArticleId={articleId}, UserId={userId}");
                    return NotFound("Reaction not found.");
                }

                _logger.LogInformation($"Reaction retrieved: ArticleId={articleId}, UserId={userId}");
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
                await _reactionService.RemoveReactionAsync(userId, articleId);
                _logger.LogInformation($"Reaction removed: ArticleId={articleId}, UserId={userId}");
                return Ok("Reaction removed successfully.");
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

                await _reactionService.UpdateReactionAsync(reaction);
                _logger.LogInformation($"Reaction updated: ArticleId={reactionDto.ArticleId}, UserId={userId}");
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
