using NewsAppAPI.DTOs;
using NewsAppAPI.Models;

namespace NewsAppAPI.Services.Interfaces
{
    public interface IArticleService
    {
        Task BulkInsertArticlesAsync(IEnumerable<NewsArticle> articles, string category);
        Task<IEnumerable<NewsArticle>> GetAllPendingArticlesAsync();
        Task<IEnumerable<NewsArticle>> GetFilteredArticlesAsync(ArticleFilter filter);
        Task<NewsArticle> GetArticleByIdAsync(int id);
        Task AddArticleAsync(NewsArticle article);
        Task DeleteArticleAsync(int id);
        Task BulkDeleteArticlesAsync(IEnumerable<int> articleIds);
        Task BulkUpdateArticlesStatusAsync(IEnumerable<int> ids, string status);
    }
}
