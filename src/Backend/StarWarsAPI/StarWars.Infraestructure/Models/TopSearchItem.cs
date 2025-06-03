namespace StarWars.Infraestructure.Models
{
    public class TopSearchItem
    {
        public string Query { get; set; }
        public int Count { get; set; }
    }

    public class HourlyStatItem
    {
        public int Hour { get; set; }
        public string HourDisplay { get; set; }
        public int Count { get; set; }
        public string Period { get; set; }
    }

    public class StatusCodeStatItem
    {
        public int StatusCode { get; set; }
        public int Count { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
    }
}
