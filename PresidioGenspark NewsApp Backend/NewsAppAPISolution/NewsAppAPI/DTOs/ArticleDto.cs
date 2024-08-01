using NewsAppAPI.Models;

namespace NewsAppAPI.DTOs
{
    public class ArticleDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public string ReadMoreUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        public int TotalLikes { get; set; }
        public int TotalDislikes { get; set; }
        public int TotalComments { get; set; }
        //public List<CommentDto> Comments { get; set; }
        //public List<ArticleReactionDto> Reactions { get; set; }
    }
}
