namespace StarWars.Domain.Domain
{
    public class PeopleListResponse
    {
        public List<PersonSummary> Results { get; set; } = new();
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public string? Previous { get; set; }
        public string? Next { get; set; }
    }
}