﻿using NewsAppAPI.Models;

namespace NewsAppAPI.Repositories.Interfaces
{
    public interface IReactionRepository
    {
        Task<Reaction> GetReactionAsync(int userId, string articleId);
        Task AddReactionAsync(Reaction reaction);
        Task RemoveReactionAsync(int userId, string articleId);
        Task AddReactionsAsync(IEnumerable<Reaction> reactions);
        Task UpdateReactionAsync(Reaction reaction);
    }
}
