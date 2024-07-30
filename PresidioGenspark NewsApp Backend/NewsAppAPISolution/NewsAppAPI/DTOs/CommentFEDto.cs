using System.ComponentModel.DataAnnotations;

namespace NewsAppAPI.DTOs
{
    public class CommentFEDto
    {
        [Required]
        public string ArticleId { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Content { get; set; }

        public int? ParentId { get; set; }
    }
}
