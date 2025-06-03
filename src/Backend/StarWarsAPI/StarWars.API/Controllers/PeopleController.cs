using Microsoft.AspNetCore.Mvc;
using StarWars.Services.Interfaces;
using StarWarsApi.Infraestructure.Model;

namespace StarWarsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PeopleController : ControllerBase
    {
        private readonly IPeopleService _peopleService;
        private readonly ILogger<PeopleController> _logger;

        public PeopleController(IPeopleService starWarsService, ILogger<PeopleController> logger)
        {
            _peopleService = starWarsService;
            _logger = logger;
        }

        /// <summary>
        /// Get a specific character by ID
        /// </summary>
        /// <param name="uid">Character ID</param>
        /// <returns>Character details</returns>
        [HttpGet("{uid}")]
        [ProducesResponseType(typeof(PersonWithMovies), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PersonWithMovies>> GetPersonById(int uid)
        {
            try
            {
                var person = await _peopleService.GetPersonByIdAsync(uid);

                if (person == null)
                {
                    return NotFound($"Character with ID {uid} not found");
                }

                return Ok(person);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for character {PersonId}", uid);
                return StatusCode(500, "Internal server error");
            }
        }


        /// Get all expanded data
        /// <summary>
        /// Search for people by name
        /// </summary>
        /// <param name="search">Name or part of name to search for</param>
        /// <returns>List of people who match the search</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(SearchResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<SearchResult>>> SearchPeopleByName([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new
                    {
                        Error = "Invalid parameter",
                        Message = "The 'name' parameter is mandatory and cannot be empty"
                    });
                }

                if (query.Length < 2)
                {
                    return BadRequest(new
                    {
                        Error = "Very short search",
                        Message = "Enter at least 2 characters to search"
                    });
                }

                _logger.LogInformation("Search by name: '{Name}'", query);

                var people = await _peopleService.SearchPeopleByNameAsync(query);

                if (people == null || !people.Any())
                {
                    return NotFound(new
                    {
                        Error = "No results",
                        Message = $"No person found with the name '{query}'"
                    });
                }

                _logger.LogInformation("Search for '{Name}' returned {Count} results", query, people.Count);
                return Ok(people);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when searching for people by name: {Name}", query);
                return StatusCode(500, new
                {
                    Error = "Internal server error",
                    Message = "Erro during the search"
                });
            }
        }


        /// <summary>
        /// Health check Endpoint 
        /// </summary>
        /// <returns>API status</returns>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
        }
    }
}