using Google;
using NewsAppAPI.Contexts;
using NewsAppAPI.DTOs;
using NewsAppAPI.Models;
using NewsAppAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace NewsAppAPI.Repositories.Classes
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly AppDbContext _context;

        public ArticleRepository(AppDbContext context)
        {
            _context = context;
        }

        #region BulkInsertArticlesAsync
        public async Task BulkInsertArticlesAsync(IEnumerable<NewsArticle> articles, string category)
        {
            // Fetch existing article IDs from the database
            var existingArticleIds = await _context.NewsArticles
                .Where(a => a.Status == "Pending")
                .Select(a => a.Id)
                .ToListAsync();

            // Convert the list of existing IDs to a HashSet
            var existingArticleIdSet = new HashSet<string>(existingArticleIds);

            // Filter out articles that already exist in the database
            var newArticles = articles
                .Where(a => !existingArticleIdSet.Contains(a.Id))
                .ToList();

            if (newArticles.Any())
            {
                await _context.NewsArticles.AddRangeAsync(newArticles);
                await _context.SaveChangesAsync();
            }
        }
        #endregion BulkInsertArticlesAsync

        #region AddArticleAsync
        public async Task AddArticleAsync(NewsArticle article)
        {
            await _context.NewsArticles.AddAsync(article);
            await _context.SaveChangesAsync();
        }
        #endregion AddArticleAsync

        #region BulkUpdateArticlesStatusAsync
        public async Task BulkUpdateArticlesStatusAsync(IEnumerable<string> ids, string status)
        {
            var articles = await _context.NewsArticles
              .Where(a => ids.Contains(a.Id))
              .ToListAsync();

            foreach (var article in articles)
            {
                article.Status = status;
            }

            _context.NewsArticles.UpdateRange(articles);
            await _context.SaveChangesAsync();
        }
        #endregion BulkUpdateArticlesStatusAsync

        #region DeleteArticlesAsync
        public async Task DeleteArticleAsync(string id)
        {
            var article = await _context.NewsArticles.FindAsync(id);
            if (article != null)
            {
                _context.NewsArticles.Remove(article);
                await _context.SaveChangesAsync();
            }
        }
        #endregion DeleteArticlesAsync

        #region BulkDeleteArticlesAsync
        public async Task BulkDeleteArticlesAsync(IEnumerable<string> articleIds)
        {
            // Fetch the articles to be deleted
            var articlesToDelete = await _context.NewsArticles
                .Where(a => articleIds.Contains(a.Id))
                .ToListAsync();

            // Check if articles were found
            if (articlesToDelete.Any())
            {
                _context.NewsArticles.RemoveRange(articlesToDelete);
                await _context.SaveChangesAsync();
            }
        }
        #endregion BulkDeleteArticlesAsync

        #region GetAllArticlesByStatusAsync
        public async Task<IEnumerable<NewsArticle>> GetAllArticlesByStatusAsync(string status)
        {
            return await _context.NewsArticles
            .Where(a => a.Status == status)
            .ToListAsync();
        }
        #endregion GetAllPendingArticlesAsync

        #region GetArticleByIdAsync
        public async Task<NewsArticle> GetArticleByIdAsync(string id)
        {
            return await _context.NewsArticles.FindAsync(id);
        }
        #endregion GetArticleByIdAsync

        #region GetFilteredArticlesAsync
        public async Task<IEnumerable<NewsArticle>> GetFilteredArticlesAsync(ArticleFilter filter)
        {
            var query = _context.NewsArticles.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Author))
            {
                query = query.Where(a => a.Author.Contains(filter.Author));
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(a => a.Date >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(a => a.Date <= filter.EndDate.Value);
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(a => a.Status == filter.Status);
            }

            if (!string.IsNullOrEmpty(filter.Title))
            {
                query = query.Where(a => a.Title.Contains(filter.Title));
            }

            if (!string.IsNullOrEmpty(filter.ContentKeyword))
            {
                query = query.Where(a => a.Content.Contains(filter.ContentKeyword));
            }

            return await query.ToListAsync();
        }
        #endregion GetFilteredArticlesAsync
    }
}
