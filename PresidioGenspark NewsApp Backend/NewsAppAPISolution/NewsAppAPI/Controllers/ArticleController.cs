﻿using Microsoft.AspNetCore.Http;
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

        [HttpGet("pending")]
        public async Task<IActionResult> GetAllPendingArticles()
        {
            var articles = await _articleService.GetAllPendingArticlesAsync();
            return Ok(articles);
        }

        [HttpGet]
        public async Task<IActionResult> GetFilteredArticles([FromQuery] ArticleFilter filter)
        {
            var articles = await _articleService.GetFilteredArticlesAsync(filter);
            return Ok(articles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetArticleById(int id)
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
        public async Task<IActionResult> DeleteArticle(int id)
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
        public async Task<IActionResult> BulkDeleteArticles([FromBody] IEnumerable<int> articleIds)
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