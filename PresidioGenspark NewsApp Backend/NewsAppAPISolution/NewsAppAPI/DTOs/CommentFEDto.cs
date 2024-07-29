namespace NewsAppAPI.DTOs
{
    public class CommentFEDto
    {
        public string ArticleId { get; set; }
        public string Content { get; set; }
        public int? ParentId { get; set; }
    }
}
