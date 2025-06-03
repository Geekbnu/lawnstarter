using StarWarsApi.Infraestructure.Model;
using StarWarsApi.Models;

namespace StarWars.Infraestucture.Interfaces;

public interface IPeopleRepository
{
    Task<List<SearchResult>> SearchByName(string name);

    Task<PersonWithMovies> GetPersonWithMoviesByUidAsync(int uid);

    Task<MongoPerson> CreateAsync(MongoPerson person);
}
