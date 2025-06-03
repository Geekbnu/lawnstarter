using System.Text.Json.Serialization;

namespace StarWarsApi.Domain;

public class PersonExpanded
{
    public StarWarsPersonProperties Properties { get; set; } = new StarWarsPersonProperties();

    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Uid { get; set; } = string.Empty;

    [JsonPropertyName("__v")]
    public int V { get; set; }
}