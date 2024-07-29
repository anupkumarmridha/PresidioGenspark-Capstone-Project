using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NewsAppAPI.DTOs;
using NewsAppAPI.Models;
using NewsAppAPI.Services.Interfaces;
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
        private readonly ILogger<CommentController> _logger;

        public CommentController(ICommentService commentService, ILogger<CommentController> logger)
        {
            _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        public async Task<IActionResult> GetCommentByIdAsync(int id)
        {
            try
            {
                var comment = await _commentService.GetCommentByIdAsync(id);

                if (comment == null)
                {
                    _logger.LogInformation($"No comment found with id: {id}");
                    return NotFound("Comment not found.");
                }

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

                await _commentService.AddCommentAsync(comment);
                _logger.LogInformation($"Comment added: ArticleId={commentDto.ArticleId}, UserId={userId}");
                return Ok("Comment added successfully.");
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

                await _commentService.UpdateCommentAsync(comment);
                _logger.LogInformation($"Comment updated: Id={id}");
                return Ok("Comment updated successfully.");
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

                await _commentService.DeleteCommentAsync(id);
                _logger.LogInformation($"Comment deleted: Id={id}");
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
