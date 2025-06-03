using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StarWarsApi.Infraestructure.Model;

public class PersonWithMovies
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("uid")]
    public int Uid { get; set; }

    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("height")]
    public string Height { get; set; }

    [BsonElement("mass")]
    public string Mass { get; set; }

    [BsonElement("hair_color")]
    public string HairColor { get; set; }

    [BsonElement("eye_color")]
    public string EyeColor { get; set; }

    [BsonElement("birth_year")]
    public string BirthYear { get; set; }

    [BsonElement("gender")]
    public string Gender { get; set; }

    [BsonElement("url")]
    public string Url { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [BsonElement("movies")]
    public List<MovieInfo> Movies { get; set; } = new List<MovieInfo>();
}
