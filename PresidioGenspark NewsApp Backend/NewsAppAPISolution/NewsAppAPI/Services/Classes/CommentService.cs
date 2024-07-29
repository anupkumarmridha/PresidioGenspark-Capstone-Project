using NewsAppAPI.Models;
using NewsAppAPI.Services.Interfaces;
using NewsAppAPI.Exceptions;
using NewsAppAPI.Repositories.Interfaces;

namespace NewsAppAPI.Services.Classes
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ILogger<CommentService> _logger;

        public CommentService(ICommentRepository commentRepository, ILogger<CommentService> logger)
        {
            _commentRepository = commentRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Comment>> GetCommentsByArticleIdAsync(string articleId)
        {
            try
            {
                return await _commentRepository.GetCommentsByArticleIdAsync(articleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching comments for article ID {ArticleId}", articleId);
                throw;
            }
        }

        public async Task<Comment> GetCommentByIdAsync(int id)
        {
            try
            {
                var comment = await _commentRepository.GetCommentByIdAsync(id);
                if (comment == null)
                {
                    throw new NotFoundException($"Comment with ID {id} not found.");
                }

                return comment;
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching comment with ID {CommentId}", id);
                throw;
            }
        }

        public async Task AddCommentAsync(Comment comment)
        {
            try
            {
                await _commentRepository.AddCommentAsync(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment");
                throw;
            }
        }

        public async Task UpdateCommentAsync(Comment comment)
        {
            try
            {
                await _commentRepository.UpdateCommentAsync(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment with ID {CommentId}", comment.Id);
                throw;
            }
        }

        public async Task DeleteCommentAsync(int id)
        {
            try
            {
                await _commentRepository.DeleteCommentAsync(id);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment with ID {CommentId}", id);
                throw;
            }
        }
    }
}
