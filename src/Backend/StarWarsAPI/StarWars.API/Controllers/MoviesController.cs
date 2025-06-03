using Microsoft.AspNetCore.Mvc;
using StarWars.Services.Interfaces;
using StarWarsApi.Infraestructure.Model;
using StarWarsApi.Metric;
using System.Diagnostics;

namespace StarWarsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly ILogger<MoviesController> _logger;
        private readonly MovieMetricsService _metrics;

        public MoviesController(
            IMovieService movieService,
            ILogger<MoviesController> logger,
            MovieMetricsService metrics)
        {
            _movieService = movieService;
            _logger = logger;
            _metrics = metrics;
        }

        [HttpGet("{uid}")]
        [ProducesResponseType(typeof(MovieWithCharacterNames), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMovie(int uid)
        {
            var stopwatch = Stopwatch.StartNew();
            var statusCode = 200;

            try
            {
                var movie = await _movieService.SearchMovieByIdWithCharacters(uid);
                
                if (movie == null)
                {
                    statusCode = 404;
                    return NotFound($"Movie with UID {uid} not found.");
                }

                return Ok(movie);
            }
            catch (Exception ex)
            {
                statusCode = 400;
                return BadRequest($"Movie search error: {ex.Message}");
            }
            finally
            {
                stopwatch.Stop();
                _metrics.RecordMovieById(uid.ToString(), stopwatch.ElapsedMilliseconds, statusCode);
            }
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(SearchResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SearchResult>> SearchMoviesByTitle([FromQuery] string query)
        {
            var stopwatch = Stopwatch.StartNew();
            var statusCode = 200;
            var resultCount = 0;

            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    statusCode = 400;
                    return BadRequest(new
                    {
                        Error = "Invalid parameter",
                        Message = "The 'query' parameter is mandatory and cannot be empty"
                    });
                }

                if (query.Length < 2)
                {
                    statusCode = 400;
                    return BadRequest(new
                    {
                        Error = "Very short search",
                        Message = "Enter at least 2 characters to search"
                    });
                }

                _logger.LogInformation("Search by title: '{Title}'", query);

                var movies = await _movieService.SearchByTitle(query);

                if (movies == null || !movies.Any())
                {
                    statusCode = 404;
                    resultCount = 0;
                    return NotFound(new
                    {
                        Error = "No results",
                        Message = $"No movie found with the title '{query}'"
                    });
                }

                resultCount = movies.Count;
                _logger.LogInformation("Search for '{Title}' returned {Count} results", query, resultCount);

                return Ok(movies);
            }
            catch (Exception ex)
            {
                statusCode = 500;
                _logger.LogError(ex, "Error when searching for movies by title: {Title}", query);
                return StatusCode(500, new
                {
                    Error = "Internal server error",
                    Message = "Erro during the search"
                });
            }
            finally
            {
                stopwatch.Stop();
                _metrics.RecordSearch(query ?? "null", stopwatch.ElapsedMilliseconds, statusCode, resultCount);
            }
        }

        /// <summary>
        /// General API statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetGeneralStats()
        {
            try
            {
                var stats = await _metrics.GetGeneralStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating general statistics");
                return StatusCode(500, new { Error = "Error generating general statistics" });
            }
        }

        /// <summary>
        /// Top searches performed
        /// </summary>
        [HttpGet("stats/top-searches")]
        public async Task<IActionResult> GetTopSearches([FromQuery] int limit = 10)
        {
            try
            {
                var topSearches = await _metrics.GetTopSearchesAsync(limit);
                return Ok(topSearches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when searching for the main research");
                return StatusCode(500, new { Error = "Error when searching for the main searches" });
            }
        }

        /// <summary>
        /// Performance statistics
        /// </summary>
        [HttpGet("stats/performance")]
        public async Task<IActionResult> GetPerformanceStats()
        {
            try
            {
                var performance = await _metrics.GetPerformanceStatsAsync();
                return Ok(performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when searching for performance statistics");
                return StatusCode(500, new { Error = "Error when searching for performance statistics" });
            }
        }

        /// <summary>
        /// Statistics by time
        /// </summary>
        [HttpGet("stats/hourly")]
        public async Task<IActionResult> GetHourlyStats()
        {
            try
            {
                var hourlyStats = await _metrics.GetHourlyStatsAsync();
                return Ok(hourlyStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when searching for statistics by time");
                return StatusCode(500, new { Error = "Error when searching for statistics by time" });
            }
        }

        /// <summary>
        /// Status codes distribution
        /// </summary>
        [HttpGet("stats/status-codes")]
        public async Task<IActionResult> GetStatusCodeStats()
        {
            try
            {
                var statusStats = await _metrics.GetStatusCodeStatsAsync();
                return Ok(statusStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when searching for status code statistics");
                return StatusCode(500, new { Error = "Error when searching for status code statistics" });
            }
        }
    }
}