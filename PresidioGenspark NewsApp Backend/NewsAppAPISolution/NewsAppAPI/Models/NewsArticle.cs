namespace NewsAppAPI.Models
{
    public class NewsArticle
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string ReadMoreUrl { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
    }
}
