using StarWarsApi.Infraestructure.Model;

namespace StarWars.Services.Interfaces
{
    public interface IMovieService
    {
        void GetAllMoviesWarmup();
        Task<MovieWithCharacterNames> SearchMovieByIdWithCharacters(int uid);
        Task<List<SearchResult>> SearchByTitle(string title);
    }
}