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
            var existingReaction = await _context.Reactions
                .FirstOrDefaultAsync(r => r.UserId == reaction.UserId && r.ArticleId == reaction.ArticleId);

            if (existingReaction != null)
            {
                throw new InvalidOperationException("Duplicate reaction detected.");
            }

            await _context.Reactions.AddAsync(reaction);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveReactionAsync(int userId, string articleId)
        {
            var reaction = await _context.Reactions
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ArticleId == articleId);

            if (reaction == null)
            {
                throw new InvalidOperationException("Reaction not found.");
            }

            _context.Reactions.Remove(reaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateReactionAsync(Reaction reaction)
        {
            var existingReaction = await _context.Reactions
                .FirstOrDefaultAsync(r => r.UserId == reaction.UserId && r.ArticleId == reaction.ArticleId);

            if (existingReaction == null)
            {
                throw new InvalidOperationException("Reaction not found.");
            }

            _context.Reactions.Update(reaction);
            await _context.SaveChangesAsync();
        }


        public async Task AddReactionsAsync(IEnumerable<Reaction> reactions)
        {
            if (reactions == null || !reactions.Any())
            {
                throw new ArgumentException("Reactions collection is null or empty.", nameof(reactions));
            }

            _context.Reactions.AddRange(reactions);
            await _context.SaveChangesAsync();
        }
    }
}
