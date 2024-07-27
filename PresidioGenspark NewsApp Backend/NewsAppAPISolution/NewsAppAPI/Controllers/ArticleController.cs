using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewsAppAPI.DTOs;
using NewsAppAPI.Models;
using NewsAppAPI.Services.Interfaces;

namespace NewsAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticleController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetAllPendingArticles(string status)
        {
            var articles = await _articleService.GetAllArticlesByStatusAsync(status);
            return Ok(articles);
        }

        [HttpGet]
        public async Task<IActionResult> GetFilteredArticles(
        [FromQuery] string? status = null,
        [FromQuery] string? category = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? author = null,
        [FromQuery] string? title = null,
        [FromQuery] string? contentKeyword = null)
        {
            var filter = new ArticleFilter
            {
                Status = status,
                Category = category,
                StartDate = startDate,
                EndDate = endDate,
                Author = author,
                Title = title,
                ContentKeyword = contentKeyword
            };

            var articles = await _articleService.GetFilteredArticlesAsync(filter);
            return Ok(articles);
        }




        [HttpGet("{id}")]
        public async Task<IActionResult> GetArticleById(string id)
        {
            try
            {
                var article = await _articleService.GetArticleByIdAsync(id);
                return Ok(article);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddArticle([FromBody] NewsArticle article)
        {
            try
            {
                await _articleService.AddArticleAsync(article);
                return CreatedAtAction(nameof(GetArticleById), new { id = article.Id }, article);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(string id)
        {
            try
            {
                await _articleService.DeleteArticleAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpDelete("bulk")]
        public async Task<IActionResult> BulkDeleteArticles([FromBody] IEnumerable<string> articleIds)
        {
            if (articleIds == null || !articleIds.Any())
            {
                return BadRequest(new { Message = "No article IDs provided for deletion." });
            }

            try
            {
                await _articleService.BulkDeleteArticlesAsync(articleIds);
                return Ok(new { Message = "Articles deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPatch("bulk/status")]
        public async Task<IActionResult> BulkUpdateArticlesStatus([FromBody] BulkUpdateRequest request)
        {
            if (request.Ids == null || !request.Ids.Any())
            {
                return BadRequest(new { Message = "No article IDs provided for status update." });
            }
            if (string.IsNullOrEmpty(request.Status))
            {
                return BadRequest(new { Message = "Status cannot be empty." });
            }

            try
            {
                await _articleService.BulkUpdateArticlesStatusAsync(request.Ids, request.Status);
                return Ok(new { Message = "Articles status updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }
    }

}
