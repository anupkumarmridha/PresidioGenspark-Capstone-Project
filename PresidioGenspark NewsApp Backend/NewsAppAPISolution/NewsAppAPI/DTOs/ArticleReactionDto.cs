using NewsAppAPI.Models;

namespace NewsAppAPI.DTOs
{
    public class ArticleReactionDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public ReactionType ReactionType { get; set; }
    }
}
