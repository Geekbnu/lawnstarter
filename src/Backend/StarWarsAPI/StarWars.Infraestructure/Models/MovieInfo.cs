using MongoDB.Bson.Serialization.Attributes;

#nullable enable
namespace StarWarsApi.Infraestructure.Model;

public class MovieInfo
{
    [BsonElement("uid")]
    public int Uid { get; set; }

    [BsonElement("title")]
    public string Title { get; set; }
}
