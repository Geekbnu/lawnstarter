using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StarWarsApi.Models;

public class MongoMovie
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("uid")]
    public int Uid { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("opening_crawl")]
    public string OpeningCrawl { get; set; } = string.Empty;

    [BsonElement("characters")]
    public List<int> Characters { get; set; } = new List<int>();

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
