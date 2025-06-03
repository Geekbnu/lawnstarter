using MongoDB.Driver;

namespace StarWarsApi.Data
{
    public class MongoContext : IMongoContext
    {
        public IMongoDatabase Database { get; }

        public MongoContext(IMongoDatabase database)
        {
            Database = database;
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return Database.GetCollection<T>(name);
        }
    }
}