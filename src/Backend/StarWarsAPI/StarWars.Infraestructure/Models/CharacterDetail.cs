
using MongoDB.Bson.Serialization.Attributes;


namespace StarWarsApi.Infraestructure.Model;

public class CharacterDetail
{
    [BsonElement("uid")]
    public int Uid { get; set; }

    [BsonElement("name")]
    public string Name { get; set; }
}
