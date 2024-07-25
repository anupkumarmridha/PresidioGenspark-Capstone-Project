namespace NewsAppAPI.DTOs
{
    public class BulkUpdateRequest
    {
        public IEnumerable<int> Ids { get; set; }
        public string Status { get; set; }
    }
}
