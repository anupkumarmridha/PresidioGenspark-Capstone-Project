namespace NewsAppAPI.DTOs
{
    public class BulkUpdateRequest
    {
        public IEnumerable<string> Ids { get; set; }
        public string Status { get; set; }
    }
}
