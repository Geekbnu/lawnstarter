using Microsoft.Extensions.Logging;
using StarWars.Infraestucture.Interfaces;
using StarWars.Services.Interfaces;
using StarWarsApi.Domain;
using StarWarsApi.Infraestructure.Model;
using StarWarsApi.Models;
using System.Text.Json;

namespace StarWarsApi.Data
{
    public class PeopleService : IPeopleService
    {
        private readonly HttpClient _httpClient;
        private readonly IPeopleRepository _repository;
        private readonly ILogger<PeopleService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private const string API_RESOURCE = "people";


        public PeopleService(
            HttpClient httpClient,
            IPeopleRepository repository,
            ILogger<PeopleService> logger)
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

        public async void GetAllWarmup()
        {
            try
            {
                var allPeople = new List<PersonExpanded>();
                var currentPage = 1;
                var totalRecords = 0;
                var totalPages = 0;
                string? nextUrl = null;

                do
                {
                    _logger.LogInformation("Fetching expanded page {Page}", currentPage);

                    var response = await _httpClient.GetAsync($"{API_RESOURCE}?page={currentPage}&limit=100&expanded=true");
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var swapiResponse = JsonSerializer.Deserialize<SwapiExpandedListResponse>(content, _jsonOptions);

                    if (swapiResponse != null)
                    {
                        foreach (var r in swapiResponse.Results)
                        {
                            await _repository.CreateAsync(new MongoPerson
                            {
                                Name = r.Properties.Name,
                                Uid = Convert.ToInt32(r.Uid),
                                Id = r.Id,
                                BirthYear=r.Properties.BirthYear,
                                Gender= r.Properties.Gender,
                                EyeColor= r.Properties.EyeColor,
                                HairColor= r.Properties.HairColor,
                                Height= r.Properties.Height,
                                Mass= r.Properties.Mass
                            });
                        }
                    }

                } while (!string.IsNullOrEmpty(nextUrl) && currentPage <= totalPages);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while fetching expanded people");
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout while fetching expanded people");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while fetching expanded people");
                throw;
            }
        }

        public async Task<PersonWithMovies?> GetPersonByIdAsync(int id)
        {
            try
            {
                return await _repository.GetPersonWithMoviesByUidAsync(id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while fetching person {PersonId}", id);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout while fetching person {PersonId}", id);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while fetching person {PersonId}", id);
                throw;
            }
        }


        public async Task<List<SearchResult>> SearchPeopleByNameAsync(string name)
        {
            try
            {
                return await _repository.SearchByName(name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pessoas por nome: {Name}", name);
                throw;
            }
        }
    }
}