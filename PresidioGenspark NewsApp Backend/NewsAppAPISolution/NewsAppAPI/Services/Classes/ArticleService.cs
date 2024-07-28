using NewsAppAPI.DTOs;
using NewsAppAPI.Models;
using NewsAppAPI.Repositories.Interfaces;
using NewsAppAPI.Services.Interfaces;

namespace NewsAppAPI.Services.Classes
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _articleRepository;

        public ArticleService(IArticleRepository articleRepository)
        {
            _articleRepository = articleRepository;
        }

        public async Task BulkInsertArticlesAsync(IEnumerable<NewsArticle> articles, string category)
        {
            await _articleRepository.BulkInsertArticlesAsync(articles, category);
        }

        public async Task<IEnumerable<NewsArticle>> GetAllArticlesByStatusAsync(string status)
        {
            return await _articleRepository.GetAllArticlesByStatusAsync(status);
        }

        public async Task<PaginatedArticlesDto> GetFilteredArticlesAsync(ArticleFilter filter, int pageNumber, int pageSize)
        {
            // Validate filter values as needed
            return await _articleRepository.GetFilteredArticlesAsync(filter, pageNumber, pageSize);
        }

        public async Task<NewsArticle> GetArticleByIdAsync(string id)
        {
            var article = await _articleRepository.GetArticleByIdAsync(id);
            if (article == null)
            {
                throw new KeyNotFoundException($"Article with ID {id} not found.");
            }
            return article;
        }

        public async Task AddArticleAsync(NewsArticle article)
        {
            // Validate article properties
            if (string.IsNullOrEmpty(article.Title) || string.IsNullOrEmpty(article.Content))
            {
                throw new ArgumentException("Title and content cannot be empty.");
            }
            await _articleRepository.AddArticleAsync(article);
        }

        public async Task DeleteArticleAsync(string id)
        {
            var article = await _articleRepository.GetArticleByIdAsync(id);
            if (article == null)
            {
                throw new KeyNotFoundException($"Article with ID {id} not found.");
            }
            await _articleRepository.DeleteArticleAsync(id);
        }

        public async Task BulkDeleteArticlesAsync(IEnumerable<string> articleIds)
        {
            if (articleIds == null || !articleIds.Any())
            {
                throw new ArgumentException("No article IDs provided for deletion.");
            }
            await _articleRepository.BulkDeleteArticlesAsync(articleIds);
        }

        public async Task BulkUpdateArticlesStatusAsync(IEnumerable<string> ids, string status)
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentException("No article IDs provided for status update.");
            }
            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentException("Status cannot be empty.");
            }
            await _articleRepository.BulkUpdateArticlesStatusAsync(ids, status);
        }
    }
}
