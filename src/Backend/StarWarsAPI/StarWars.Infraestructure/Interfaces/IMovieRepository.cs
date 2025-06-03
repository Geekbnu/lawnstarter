using StarWarsApi.Infraestructure.Model;
using StarWarsApi.Models;

namespace StarWars.Infraestucture.Interfaces;

public interface IMovieRepository
{

    Task<List<SearchResult>> SearchByTitleAsync(string title);

    Task<MovieWithCharacterNames> GetMovieWithCharacterNamesByUidAsync(int uid);

    Task<MongoMovie> CreateAsync(MongoMovie movie);
}
