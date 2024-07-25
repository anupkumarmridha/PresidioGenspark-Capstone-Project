namespace NewsAppAPI.Models
{

    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Role { get; set; }
        public string GoogleId { get; set; }
        public string GivenName { get; set; } 
        public string FamilyName { get; set; }
        public string Picture { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
