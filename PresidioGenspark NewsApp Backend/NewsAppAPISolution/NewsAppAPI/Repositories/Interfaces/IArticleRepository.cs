using NewsAppAPI.DTOs;
using NewsAppAPI.Models;

namespace NewsAppAPI.Repositories.Interfaces
{
    public interface IArticleRepository
    {
        Task<IEnumerable<NewsArticle>> GetAllPendingArticlesAsync();
        Task<IEnumerable<NewsArticle>> GetFilteredArticlesAsync(ArticleFilter filter);
        Task<NewsArticle> GetArticleByIdAsync(int id);
        Task AddArticleAsync(NewsArticle article);
        Task DeleteArticleAsync(int id);
        public Task BulkDeleteArticlesAsync(IEnumerable<int> articleIds);
        Task BulkUpdateArticlesStatusAsync(IEnumerable<int> ids, string status);
    }

}
