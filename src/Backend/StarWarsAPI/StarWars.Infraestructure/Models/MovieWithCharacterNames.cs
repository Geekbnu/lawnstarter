using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

#nullable enable
namespace StarWarsApi.Infraestructure.Model;

public class MovieWithCharacterNames
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("uid")]
    public int Uid { get; set; }

    [BsonElement("title")]
    public string Title { get; set; }

    [BsonElement("opening_crawl")]
    public string OpeningCrawl { get; set; }

    [BsonElement("characters")]
    public List<CharacterDetail> Characters { get; set; } = new List<CharacterDetail>();

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
