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
        private readonly ILogger<ArticleRepository> _logger;

        public ArticleRepository(AppDbContext context, ILogger<ArticleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region BulkInsertArticlesAsync
        public async Task BulkInsertArticlesAsync(IEnumerable<NewsArticle> articles, string category)
        {
            // Fetch existing articles from the database
            var existingArticles = await _context.NewsArticles
                .Where(a => a.Status == "Pending")
                .ToListAsync();

            // Use a set to track existing article IDs for quick lookup
            var existingArticleIdSet = new HashSet<string>(existingArticles.Select(a => a.Id));

            // Use a set to track content hashes of existing articles for additional duplicate check
            var existingContentHashes = new HashSet<string>(existingArticles.Select(a => a.Content.GetHashCode().ToString()));

            // Filter out articles that already exist based on ID or content hash
            var newArticles = articles
                .Where(a => !existingArticleIdSet.Contains(a.Id) && !existingContentHashes.Contains(a.Content.GetHashCode().ToString()))
                .ToList();

            if (newArticles.Any())
            {
                await _context.NewsArticles.AddRangeAsync(newArticles);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Inserting {Count} new articles for category {Category}.", newArticles.Count, category);
            }
            else
            {
                _logger.LogInformation("No new articles to insert for category {Category}.", category);
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
        public async Task<ArticleDto> GetArticleByIdAsync(string id)
        {
            var article = await _context.NewsArticles
        .Include(a => a.Comments) // Eager load comments
        .Include(a => a.Reactions) // Eager load reactions
        .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                throw new KeyNotFoundException("Article not found.");
            }

            var totalLikes = article.Reactions.Count(r => r.ReactionType == ReactionType.Like);
            var totalDislikes = article.Reactions.Count(r => r.ReactionType == ReactionType.Dislike);
            var totalComments = article.Comments.Count;

            var articleDto = new ArticleDto
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                CreatedAt = article.Date,
                TotalLikes = totalLikes,
                TotalDislikes = totalDislikes,
                TotalComments = totalComments,
                Comments = article.Comments.Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UserId = c.UserId,
                    ParentId = c.ParentId,
                    Replies = c.Replies.Select(r => new CommentDto
                    {
                        Id = r.Id,
                        Content = r.Content,
                        CreatedAt = r.CreatedAt,
                        UserId = r.UserId,
                        ParentId = r.ParentId
                    }).ToList()
                }).ToList()
            };

            return articleDto;
        }
        #endregion GetArticleByIdAsync

        #region GetFilteredArticlesAsync
        public async Task<PaginatedArticlesDto> GetFilteredArticlesAsync(ArticleFilter filter, int pageNumber, int pageSize)
        {
            var query = _context.NewsArticles.AsQueryable();

            // Apply filters
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

            if (!string.IsNullOrEmpty(filter.Status) && filter.Status != "all")
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

            if (!string.IsNullOrEmpty(filter.Category))
            {
                query = query.Where(a => a.Category == filter.Category);
            }

            // Order by date descending
            query = query.OrderByDescending(a => a.Date);

            // Calculate total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var paginatedArticles = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedArticlesDto
            {
                TotalCount = totalCount,
                Articles = paginatedArticles
            };
        }

        #endregion GetFilteredArticlesAsync
    }
}
