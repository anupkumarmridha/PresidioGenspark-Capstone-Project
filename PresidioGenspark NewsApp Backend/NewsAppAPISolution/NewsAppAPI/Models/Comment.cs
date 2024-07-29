namespace NewsAppAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string ArticleId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        // Nullable ParentId for replies
        public int? ParentId { get; set; }
        public Comment Parent { get; set; }

        // Navigation property for replies
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
        
        // Navigation property for the article
        public NewsArticle NewsArticle { get; set; }
    }
}
