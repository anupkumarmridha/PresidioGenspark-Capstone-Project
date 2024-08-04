using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewsAppAPI.DTOs;
using NewsAppAPI.Models;
using NewsAppAPI.Services.Interfaces;
using NewsAppAPI.Cache;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace NewsAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly IArticleService _articleService;
        private readonly ICacheService _cacheService;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

        public ArticleController(IArticleService articleService, ICacheService cacheService)
        {
            _articleService = articleService ?? throw new ArgumentNullException(nameof(articleService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetAllPendingArticles(string status)
        {
            var cacheKey = $"articles-status-{status}";
            var cachedArticles = await _cacheService.GetAsync(cacheKey) as IEnumerable<NewsArticle>;

            if (cachedArticles != null)
            {
                return Ok(cachedArticles);
            }

            var articles = await _articleService.GetAllArticlesByStatusAsync(status);
            await _cacheService.SetAsync(cacheKey, articles, _cacheExpiration);

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
            [FromQuery] string? contentKeyword = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
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

            //var cacheKey = $"articles-filter-{pageNumber}-{pageSize}-{status}-{category}-{startDate}-{endDate}-{author}-{title}-{contentKeyword}";
            //var cachedArticles = await _cacheService.GetAsync(cacheKey) as PaginatedArticlesDto;

            //if (cachedArticles != null)
            //{
            //    return Ok(cachedArticles);
            //}

            var paginatedArticles = await _articleService.GetFilteredArticlesAsync(filter, pageNumber, pageSize);
            //await _cacheService.SetAsync(cacheKey, paginatedArticles, _cacheExpiration);

            return Ok(paginatedArticles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetArticleById(string id)
        {
            var cacheKey = $"article-{id}";
            var cachedArticle = await _cacheService.GetAsync(cacheKey) as NewsArticle;

            if (cachedArticle != null)
            {
                return Ok(cachedArticle);
            }

            try
            {
                var article = await _articleService.GetArticleByIdAsync(id);
                await _cacheService.SetAsync(cacheKey, article, _cacheExpiration);

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
            if (article == null)
            {
                return BadRequest(new { Message = "Article is null." });
            }

            try
            {
                await _articleService.AddArticleAsync(article);

                // Invalidate cache
                await _cacheService.RemoveAsync($"articles-status-*");
                await _cacheService.RemoveAsync($"articles-filter-*");
                await _cacheService.RemoveAsync($"article-{article.Id}");

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

                // Invalidate cache
                await _cacheService.RemoveAsync($"article-{id}");

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

                // Invalidate cache
                foreach (var id in articleIds)
                {
                    await _cacheService.RemoveAsync($"article-{id}");
                }
                await _cacheService.RemoveAsync($"articles-status-*");
                await _cacheService.RemoveAsync($"articles-filter-*");

                return Ok(new { Message = "Articles deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }
        [Authorize(Roles = "Admin")]
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

                // Invalidate cache
                foreach (var id in request.Ids)
                {
                    await _cacheService.RemoveAsync($"article-{id}");
                }
                await _cacheService.RemoveAsync($"articles-status-*");
                await _cacheService.RemoveAsync($"articles-filter-*");

                return Ok(new { Message = "Articles status updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
