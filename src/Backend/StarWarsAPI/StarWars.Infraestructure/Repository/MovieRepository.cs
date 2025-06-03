using MongoDB.Bson;
using MongoDB.Driver;
using StarWars.Infraestucture.Interfaces;
using StarWarsApi.Data;
using StarWarsApi.Infraestructure.Model;
using StarWarsApi.Models;
using System.Linq.Expressions;


namespace StarWars.Infraestructure.Repository;

public class MovieRepository : IMovieRepository
{
    private readonly IMongoCollection<MongoMovie> _collection;

    public MovieRepository(IMongoContext context)
    {
        _collection = context.GetCollection<MongoMovie>("movies");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        _collection.Indexes.CreateOneAsync(new CreateIndexModel<MongoMovie>(Builders<MongoMovie>.IndexKeys.Ascending((Expression<Func<MongoMovie, object>>)(x => x.Uid)), new CreateIndexOptions()
        {
            Unique = new bool?(true)
        }));
        _collection.Indexes.CreateOneAsync(new CreateIndexModel<MongoMovie>(Builders<MongoMovie>.IndexKeys.Text((Expression<Func<MongoMovie, object>>)(x => x.Title))));
    }


    public async Task<List<SearchResult>> SearchByTitleAsync(string title)
    {
        FilterDefinition<MongoMovie> filter = Builders<MongoMovie>.Filter.Text(title);
        List<MongoMovie> movies = await _collection.Find(filter).ToListAsync();
        List<SearchResult> list = movies.Select(movie => new SearchResult()
        {
            Name = movie.Title,
            Type = "Movie",
            Uid = movie.Uid
        }).ToList();
        filter = null;
        movies = null;
        return list;
    }

    public async Task<MongoMovie> CreateAsync(MongoMovie movie)
    {
        movie.CreatedAt = DateTime.UtcNow;
        movie.UpdatedAt = DateTime.UtcNow;
        await _collection.InsertOneAsync(movie);
        return movie;
    }

    public async Task<MovieWithCharacterNames> GetMovieWithCharacterNamesByUidAsync(int uid)
    {
        BsonDocument[] pipeline = new BsonDocument[3]
        {
      new BsonDocument("$match",  new BsonDocument(nameof (uid), (BsonValue) uid)),
      new BsonDocument("$lookup",  new BsonDocument()
      {
        {
          "from",
          (BsonValue) "people"
        },
        {
          "localField",
          (BsonValue) "characters"
        },
        {
          "foreignField",
          (BsonValue) nameof (uid)
        },
        {
          "as",
          (BsonValue) "characterDetails"
        }
      }),
      new BsonDocument("$project",  new BsonDocument()
      {
        {
          "_id",
          (BsonValue) 1
        },
        {
          nameof (uid),
          (BsonValue) 1
        },
        {
          "title",
          (BsonValue) 1
        },
        {
          "opening_crawl",
          (BsonValue) 1
        },
        {
          "created_at",
          (BsonValue) 1
        },
        {
          "updated_at",
          (BsonValue) 1
        },
        {
          "characters",
           new BsonDocument("$map",  new BsonDocument()
          {
            {
              "input",
              (BsonValue) "$characterDetails"
            },
            {
              "as",
              (BsonValue) "character"
            },
            {
              "in",
               new BsonDocument()
              {
                {
                  nameof (uid),
                  (BsonValue) "$$character.uid"
                },
                {
                  "name",
                  (BsonValue) "$$character.name"
                }
              }
            }
          })
        }
      })
        };
        MovieWithCharacterNames result = await _collection.Aggregate((PipelineDefinition<MongoMovie, MovieWithCharacterNames>)pipeline).FirstOrDefaultAsync();
        MovieWithCharacterNames characterNamesByUidAsync = result;
        pipeline = null;
        result = null;
        return characterNamesByUidAsync;
    }
}
