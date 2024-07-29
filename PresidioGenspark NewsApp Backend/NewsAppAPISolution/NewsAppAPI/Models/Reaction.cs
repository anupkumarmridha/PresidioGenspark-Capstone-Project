namespace NewsAppAPI.Models
{
    public class Reaction
    {
        public int Id { get; set; }
        public string ArticleId { get; set; }
        public int UserId { get; set; }
        public ReactionType ReactionType { get; set; }
        public User User { get; set; }

        public NewsArticle NewsArticle { get; set; }
    }

    public enum ReactionType
    {
        Like,
        Dislike
    }
}
