namespace NewsAppAPI.DTOs
{
    public class ArticleDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalLikes { get; set; }
        public int TotalDislikes { get; set; }
        public int TotalComments { get; set; }
        public List<CommentDto> Comments { get; set; }
    }
}
