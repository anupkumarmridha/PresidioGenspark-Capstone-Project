namespace NewsAppAPI.DTOs
{
    public class ArticleFilter
    {
        public string? Author { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public string? Title { get; set; }
        public string? ContentKeyword { get; set; }
        public string? Category { get; set; }
    }

}
