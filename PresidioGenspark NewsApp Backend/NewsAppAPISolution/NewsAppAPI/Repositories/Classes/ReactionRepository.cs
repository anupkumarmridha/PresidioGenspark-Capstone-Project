using Microsoft.EntityFrameworkCore;
using NewsAppAPI.Contexts;
using NewsAppAPI.Models;
using NewsAppAPI.Repositories.Interfaces;

namespace NewsAppAPI.Repositories.Classes
{
    public class ReactionRepository : IReactionRepository
    {
        private readonly AppDbContext _context;

        public ReactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Reaction> GetReactionAsync(int userId, string articleId)
        {
            return await _context.Reactions
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ArticleId == articleId);
        }

        public async Task AddReactionAsync(Reaction reaction)
        {
            await _context.Reactions.AddAsync(reaction);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveReactionAsync(int userId, string articleId)
        {
            var reaction = await GetReactionAsync(userId, articleId);
            if (reaction != null)
            {
                _context.Reactions.Remove(reaction);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateReactionAsync(Reaction reaction)
        {
            _context.Reactions.Update(reaction);
            await _context.SaveChangesAsync();
        }
    }
}
