using System.Text.Json.Serialization;

namespace StarWarsApi.Domain;

public class FilmProperties
{
    public List<string> Characters { get; set; } = new List<string>();

    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("opening_crawl")]
    public string OpeningCrawl { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
}