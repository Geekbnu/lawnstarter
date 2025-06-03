using StarWarsApi.Infraestructure.Model;

namespace StarWars.Services.Interfaces
{
    public interface IPeopleService
    {
        void GetAllWarmup();
        Task<PersonWithMovies?> GetPersonByIdAsync(int id);
        Task<List<SearchResult>> SearchPeopleByNameAsync(string name);
    }
}