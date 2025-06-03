using MongoDB.Bson.Serialization.Attributes;

namespace StarWarsApi.Infraestructure.Model;

public class SearchResult
{
    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("uid")]
    public int Uid { get; set; }

    public string Type { get; set; } = "People";
}
