using NewsAppAPI.Models;
namespace NewsAppAPI.DTOs
{
    public class PaginatedArticlesDto
    {
        public int TotalCount { get; set; }
        public IEnumerable<NewsArticle> Articles { get; set; }
    }
}
