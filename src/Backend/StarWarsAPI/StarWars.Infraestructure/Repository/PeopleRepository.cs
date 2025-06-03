using MongoDB.Bson;
using MongoDB.Driver;
using StarWars.Infraestucture.Interfaces;
using StarWarsApi.Data;
using StarWarsApi.Infraestructure.Model;
using StarWarsApi.Models;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace StarWars.Infraestructure.Repository;
public class PeopleRepository : IPeopleRepository
{
    private readonly IMongoCollection<MongoPerson> _collection;
    public PeopleRepository(IMongoContext context)
    {
        _collection = context.GetCollection<MongoPerson>("people");
        CreateIndexes();
    }
    private void CreateIndexes()
    {
        _collection.Indexes.CreateOneAsync(new CreateIndexModel<MongoPerson>(Builders<MongoPerson>.IndexKeys.Ascending((Expression<Func<MongoPerson, object>>)(x => x.Uid)), new CreateIndexOptions()
        {
            Unique = new bool?(true)
        }));
        _collection.Indexes.CreateOneAsync(new CreateIndexModel<MongoPerson>(Builders<MongoPerson>.IndexKeys.Text((Expression<Func<MongoPerson, object>>)(x => x.Name))));
    }

    public async Task<List<SearchResult>> SearchByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return new List<SearchResult>();
        FilterDefinition<MongoPerson> filter = Builders<MongoPerson>.Filter.Regex(p => p.Name, new BsonRegularExpression(Regex.Escape(name), "i"));
        List<MongoPerson> people = await _collection.Find(filter).ToListAsync();
        List<SearchResult> list = people.Select(person => new SearchResult
        {
            Name = person.Name,
            Type = "People",
            Uid = person.Uid
        }).ToList();
        return list;
    }
    public async Task<MongoPerson> CreateAsync(MongoPerson person)
    {
        person.CreatedAt = DateTime.UtcNow;
        person.UpdatedAt = DateTime.UtcNow;
        await _collection.InsertOneAsync(person);
        return person;
    }

    public async Task<PersonWithMovies> GetPersonWithMoviesByUidAsync(int uid)
    {
        BsonDocument[] pipeline = new BsonDocument[3]
        {
            new BsonDocument("$match",  new BsonDocument(nameof(uid), (BsonValue) uid)),
                new BsonDocument("$lookup",  new BsonDocument()
                {
                    {
                        "from",
                        (BsonValue)
                        "movies"
                    },
                    {
                        "localField",
                        (BsonValue) nameof(uid)
                    },
                    {
                        "foreignField",
                        (BsonValue)
                        "characters"
                    },
                    {
                        "as",
                        (BsonValue)
                        "movieDetails"
                    }
                }),
                new BsonDocument("$project",  new BsonDocument()
                {
                    {
                        "_id",
                        (BsonValue) 1
                    },
                    {
                        nameof(uid),
                        (BsonValue) 1
                    },
                    {
                        "name",
                        (BsonValue) 1
                    },
                    {
                        "height",
                        (BsonValue) 1
                    },
                    {
                        "mass",
                        (BsonValue) 1
                    },
                    {
                        "hair_color",
                        (BsonValue) 1
                    },
                    {
                        "eye_color",
                        (BsonValue) 1
                    },
                    {
                        "birth_year",
                        (BsonValue) 1
                    },
                    {
                        "gender",
                        (BsonValue) 1
                    },
                    {
                        "url",
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
                        "movies",
                         new BsonDocument("$map",  new BsonDocument()
                        {
                            {
                                "input",
                                (BsonValue)
                                "$movieDetails"
                            },
                            {
                                "as",
                                (BsonValue)
                                "movie"
                            },
                            {
                                "in",
                                 new BsonDocument()
                                {
                                    {
                                        nameof(uid),
                                            (BsonValue)
                                        "$$movie.uid"
                                    },
                                    {
                                        "title",
                                        (BsonValue)
                                        "$$movie.title"
                                    }
                                }
                            }
                        })
                    }
                })
        };
        PersonWithMovies result = await _collection.Aggregate((PipelineDefinition<MongoPerson, PersonWithMovies>)pipeline).FirstOrDefaultAsync();
        PersonWithMovies moviesByUidAsync = result;
        pipeline = null;
        result = null;
        return moviesByUidAsync;
    }
}