namespace NewsAppAPI.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string ArticleId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int? ParentId { get; set; }
        public List<CommentDto> Replies { get; set; }
    }
}
