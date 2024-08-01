using NewsAppAPI.DTOs;
using NewsAppAPI.Models;

namespace NewsAppAPI.Services.Interfaces
{
    public interface IReactionService
    {
        Task<Reaction> GetReactionAsync(int userId, string articleId);
        Task AddReactionAsync(Reaction reaction);
        Task RemoveReactionAsync(int userId, string articleId);
        Task UpdateReactionAsync(Reaction reaction);
        Task<IEnumerable<ArticleReactionDto>> GetAllReactionsByArticleAsync(string articleId);
    }
}
