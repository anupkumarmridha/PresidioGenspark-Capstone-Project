using NewsAppAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace NewsAppAPI.DTOs
{
    public class ReactionDto
    {
        [Required]
        public string ArticleId { get; set; }

        [Required]
        [EnumDataType(typeof(ReactionType))]
        public ReactionType ReactionType { get; set; }
    }
}
