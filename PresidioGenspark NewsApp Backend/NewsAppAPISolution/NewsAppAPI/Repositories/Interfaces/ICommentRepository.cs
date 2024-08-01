using NewsAppAPI.DTOs;
using NewsAppAPI.Models;

namespace NewsAppAPI.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task<IEnumerable<CommentDto>> GetCommentsByArticleIdAsync(string articleId);
        Task<Comment> GetCommentByIdAsync(int id);
        Task AddCommentAsync(Comment comment);
        Task AddCommentsAsync(IEnumerable<Comment> comments);
        Task UpdateCommentAsync(Comment comment);
        Task DeleteCommentAsync(int id);
    }
}
