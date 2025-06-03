using Microsoft.Extensions.Logging;
using StarWars.Infraestucture.Interfaces;
using StarWars.Services.Interfaces;
using StarWarsApi.Domain;
using StarWarsApi.Infraestructure.Model;
using StarWarsApi.Models;
using System.Text.Json;

namespace StarWarsApi.Data
{
    public class MovieService : IMovieService
    {
        private readonly HttpClient _httpClient;
        private readonly IMovieRepository _repository;
        private readonly ILogger<IMovieService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        private const string API_RESOURCE = "films";

        public MovieService(
            HttpClient httpClient,
            IMovieRepository repository,
            ILogger<IMovieService> logger)
        {
            _httpClient = httpClient;
            _repository = repository;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true
            };
        }

        public async void GetAllMoviesWarmup()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{API_RESOURCE}?page={1}&limit=100&expanded=true");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var swapiResponse = JsonSerializer.Deserialize<SwapiFilmsResponse>(content, _jsonOptions);

                if (swapiResponse != null)
                {
                    foreach (var r in swapiResponse.Result)
                    {
                        await _repository.CreateAsync(new MongoMovie
                        {
                            Characters = r.Properties.Characters.Select(x => Convert.ToInt32(x.Replace("https://www.swapi.tech/api/people/", ""))).ToList(),
                            Uid = Convert.ToInt32(r.Uid),
                            Id = r.Id,
                            Title = r.Properties.Title,
                            CreatedAt = DateTime.Now,
                            OpeningCrawl = r.Properties.OpeningCrawl,
                        });
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro HTTP ao buscar filmes da API externa");
                throw new InvalidOperationException("Erro ao comunicar com a API externa de filmes", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erro de deserialização JSON ao processar resposta de filmes");
                throw new InvalidOperationException("Erro ao processar dados de filmes", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao buscar filmes");
                throw;
            }
        }

        public async Task<List<SearchResult>> SearchByTitle(string title)
        {
            try
            {
                return await _repository.SearchByTitleAsync(title);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<MovieWithCharacterNames> SearchMovieByIdWithCharacters(int uid)
        {
            return await _repository.GetMovieWithCharacterNamesByUidAsync(uid);
        }
    }
}