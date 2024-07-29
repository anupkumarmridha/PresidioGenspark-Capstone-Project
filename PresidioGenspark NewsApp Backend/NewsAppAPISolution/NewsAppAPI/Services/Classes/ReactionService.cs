using NewsAppAPI.Models;
using NewsAppAPI.Repositories.Interfaces;
using NewsAppAPI.Services.Interfaces;


namespace NewsAppAPI.Services.Classes
{
    public class ReactionService : IReactionService
    {
        private readonly ILogger<ReactionService> _logger;
        private readonly IReactionRepository _reactionRepository;

        public ReactionService(ILogger<ReactionService> logger, IReactionRepository reactionRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _reactionRepository = reactionRepository ?? throw new ArgumentNullException(nameof(reactionRepository));
        }

        public async Task AddReactionAsync(Reaction reaction)
        {
            try
            {
                if (reaction == null)
                {
                    _logger.LogWarning("Attempted to add a null reaction.");
                    throw new ArgumentNullException(nameof(reaction));
                }

                await _reactionRepository.AddReactionAsync(reaction);
                _logger.LogInformation($"Reaction added successfully for articleId: {reaction.ArticleId} by userId: {reaction.UserId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a reaction.");
                throw; // Re-throw the exception after logging it
            }
        }

        public async Task<Reaction> GetReactionAsync(int userId, string articleId)
        {
            try
            {
                var reaction = await _reactionRepository.GetReactionAsync(userId, articleId);
                if (reaction == null)
                {
                    _logger.LogInformation($"No reaction found for articleId: {articleId} by userId: {userId}");
                }
                else
                {
                    _logger.LogInformation($"Reaction retrieved for articleId: {articleId} by userId: {userId}");
                }
                return reaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving a reaction.");
                throw; 
            }
        }

        public async Task RemoveReactionAsync(int userId, string articleId)
        {
            try
            {
                await _reactionRepository.RemoveReactionAsync(userId, articleId);
                _logger.LogInformation($"Reaction removed successfully for articleId: {articleId} by userId: {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing a reaction.");
                throw; 
            }
        }

        public async Task UpdateReactionAsync(Reaction reaction)
        {
            try
            {
                if (reaction == null)
                {
                    _logger.LogWarning("Attempted to update a null reaction.");
                    throw new ArgumentNullException(nameof(reaction));
                }

                await _reactionRepository.UpdateReactionAsync(reaction);
                _logger.LogInformation($"Reaction updated successfully for articleId: {reaction.ArticleId} by userId: {reaction.UserId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating a reaction.");
                throw;
            }
        }
    }
}
