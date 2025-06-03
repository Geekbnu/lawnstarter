using MongoDB.Driver;

namespace StarWarsApi.Data
{
    public interface IMongoContext
    {
        IMongoDatabase Database { get; }
        IMongoCollection<T> GetCollection<T>(string name);
    }
}