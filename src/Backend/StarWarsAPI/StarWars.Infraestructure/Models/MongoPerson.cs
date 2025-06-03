using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StarWarsApi.Models;

public class MongoPerson
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("uid")]
    public int Uid { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("height")]
    public string Height { get; set; } = string.Empty;

    [BsonElement("mass")]
    public string Mass { get; set; } = string.Empty;

    [BsonElement("hair_color")]
    public string HairColor { get; set; } = string.Empty;

    [BsonElement("eye_color")]
    public string EyeColor { get; set; } = string.Empty;

    [BsonElement("birth_year")]
    public string BirthYear { get; set; } = string.Empty;

    [BsonElement("gender")]
    public string Gender { get; set; } = string.Empty;

    [BsonElement("films")]
    public List<string> Films { get; set; } = new List<string>();

    [BsonElement("url")]
    public string Url { get; set; } = string.Empty;

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
