using System.ComponentModel.DataAnnotations;

namespace NewsAppAPI.DTOs
{
    public class KafkaMessageDto
    {
        [Required]
        public string Topic { get; set; }
        [Required]
        public string Message { get; set; }
        [Required]
        public string Operation { get; set; }
    }
}
