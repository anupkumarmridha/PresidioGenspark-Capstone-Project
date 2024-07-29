using NewsAppAPI.Models;

namespace NewsAppAPI.DTOs
{
    public class ReactionDto
    {
        public string ArticleId { get; set; }
        public ReactionType ReactionType { get; set; }
    }
}
