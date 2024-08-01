using Microsoft.EntityFrameworkCore;
using NewsAppAPI.Contexts;
using NewsAppAPI.Models;
using NewsAppAPI.Repositories.Interfaces;

namespace NewsAppAPI.Repositories.Classes
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _context;

        public CommentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetCommentsByArticleIdAsync(string articleId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Replies)
                .Where(c => c.ArticleId == articleId)
                .ToListAsync();
        }

        public async Task<Comment> GetCommentByIdAsync(int id)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddCommentAsync(Comment comment)
        {
            if (comment.ParentId.HasValue)
            {
                // Ensure ParentId exists in the database if needed
                var parentComment = await _context.Comments.FindAsync(comment.ParentId.Value);
                if (parentComment == null)
                {
                   
                    throw new InvalidOperationException("Invalid ParentId specified.");
                }
            }

            try
            {
                await _context.Comments.AddAsync(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task UpdateCommentAsync(Comment comment)
        {
            var existingComment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            if (existingComment == null)
            {
                throw new InvalidOperationException("Comment not found.");
            }

            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCommentAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                throw new InvalidOperationException("Comment not found.");
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }


        public async Task AddCommentsAsync(IEnumerable<Comment> comments)
        {
            if (comments == null || !comments.Any())
            {
                throw new ArgumentException("Comments collection is null or empty.", nameof(comments));
            }

            _context.Comments.AddRange(comments);
            await _context.SaveChangesAsync();
        }
    }
}
