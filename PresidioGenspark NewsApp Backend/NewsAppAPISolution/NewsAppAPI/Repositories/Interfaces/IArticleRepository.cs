using NewsAppAPI.DTOs;
using NewsAppAPI.Models;

namespace NewsAppAPI.Repositories.Interfaces
{
    public interface IArticleRepository
    {
        Task BulkInsertArticlesAsync(IEnumerable<NewsArticle> articles, string category);
        Task<IEnumerable<NewsArticle>> GetAllArticlesByStatusAsync(string status);
        Task<PaginatedArticlesDto> GetFilteredArticlesAsync(ArticleFilter filter, int pageNumber, int pageSize);
        Task<NewsArticle> GetArticleByIdAsync(string id);
        Task AddArticleAsync(NewsArticle article);
        Task DeleteArticleAsync(string id);
        public Task BulkDeleteArticlesAsync(IEnumerable<string> articleIds);
        Task BulkUpdateArticlesStatusAsync(IEnumerable<string> ids, string status);
    }

}
