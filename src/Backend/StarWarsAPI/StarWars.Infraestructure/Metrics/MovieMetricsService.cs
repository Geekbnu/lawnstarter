using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prometheus;
using StarWars.Infraestructure.Models;
using System.Text.Json;

namespace StarWarsApi.Metric
{
    public class MovieMetricsService
    {
        private string _currentSession;
        private Counter? _searchQueries;
        private Counter? _movieByIdQueries;
        private Histogram? _searchDuration;
        private Counter? _searchesByHour;
        private Counter? _searchResults;
        private readonly string _victoriaMetricsUrl;
        public string CurrentSession => _currentSession;

        private HttpClient _httpClient;
        private ILogger<MovieMetricsService> _logger;

        public MovieMetricsService(IConfiguration configuration, ILogger<MovieMetricsService> logger, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            InitializeMetrics();
            _victoriaMetricsUrl = configuration["Metrics:VictoriaMetrics:Url"] ?? "http://localhost:8428";
        }

        private void InitializeMetrics()
        {
            _currentSession = $"s_{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss}";

            _searchQueries = Metrics.CreateCounter(
                $"movie_search_queries_total_{_currentSession}",
                "Total search queries",
                new[] { "query", "status_code" });

            _movieByIdQueries = Metrics.CreateCounter(
                $"movie_byid_queries_total_{_currentSession}",
                "Total movie by ID queries",
                new[] { "status_code" });

            _searchesByHour = Metrics.CreateCounter(
                $"movie_searches_by_hour_total_{_currentSession}",
                "Searches by hour",
                new[] { "hour" });

            _searchResults = Metrics.CreateCounter(
                $"movie_search_results_total_{_currentSession}",
                "Search results count ranges",
                new[] { "range" });

            _searchDuration = Metrics.CreateHistogram(
                $"movie_search_duration_seconds_{_currentSession}",
                "Search duration in seconds",
                new[] { "session" });
        }

        public void RecordSearch(string query, double durationMs, int statusCode, int resultCount)
        {
            var normalizedQuery = NormalizeQuery(query);
            _searchQueries.WithLabels(normalizedQuery, statusCode.ToString()).Inc();
            _searchDuration.Observe(durationMs / 1000.0);
            _searchesByHour.WithLabels(DateTime.Now.Hour.ToString()).Inc();
            var resultRange = GetResultCountRange(resultCount);
            _searchResults.WithLabels(resultRange).Inc();
        }

        public void RecordMovieById(string movieId, double durationMs, int statusCode)
        {
            _movieByIdQueries.WithLabels(statusCode.ToString()).Inc();
            _searchDuration.Observe(durationMs / 1000.0);
        }

        private string NormalizeQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return "empty";

            var normalized = query.ToLower().Trim();
            if (normalized.Length > 20)
                normalized = normalized.Substring(0, 20) + "...";

            return normalized;
        }

        public async Task<JsonDocument> QueryAsync(string query)
        {
            var encodedQuery = Uri.EscapeDataString(query);
            var url = $"{_victoriaMetricsUrl}/api/v1/query?query={encodedQuery}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(json);
        }

        public async Task<JsonDocument> QueryRangeAsync(string query, DateTime start, DateTime end, string step = "1m")
        {
            var startUnix = ((DateTimeOffset)start).ToUnixTimeSeconds();
            var endUnix = ((DateTimeOffset)end).ToUnixTimeSeconds();
            var encodedQuery = Uri.EscapeDataString(query);

            var url = $"{_victoriaMetricsUrl}/api/v1/query_range" +
                     $"?query={encodedQuery}" +
                     $"&start={startUnix}" +
                     $"&end={endUnix}" +
                     $"&step={step}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(json);
        }


        public async Task<bool> DeleteAllMetricsAsync()
        {
            try
            {
                //var url = $"{_victoriaMetricsUrl}/api/v1/admin/tsdb/delete_series?match[]="+"{__name__ !=\"\"}";
                //var response = await _httpClient.PostAsync(url, null);

                InitializeMetrics();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        /// <summary>
        /// Get general statistics
        /// </summary>
        public async Task<object> GetGeneralStatsAsync()
        {
            try
            {
                var overview = await GetOverviewStatsAsync();
                var performance = await GetPerformanceStatsAsync();
                var usage = await GetUsageStatsAsync();

                return new
                {
                    overview,
                    performance,
                    usage,
                    generatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar estatísticas gerais");
                throw new InvalidOperationException("Erro ao gerar estatísticas gerais", ex);
            }
        }

        /// <summary>
        /// Get the best searches
        /// </summary>
        public async Task<object> GetTopSearchesAsync(int limit = 10)
        {
            try
            {
                var query = $"topk({limit}, sum by (query) (movie_search_queries_total_{_currentSession}))";
                var result = await QueryAsync(query);

                var topSearches = ExtractTopSearches(result);
                var total = topSearches.Sum(s => s.Count);

                var withPercentages = topSearches.Select(s => new
                {
                    query = s.Query,
                    count = s.Count,
                    normalized = s.Query?.Length > 15 ? s.Query[..15] + "..." : s.Query,
                    percentage = total > 0 ? Math.Round((double)s.Count / total * 100, 2) : 0
                }).ToList();

                result.Dispose();

                return new
                {
                    totalSearches = total,
                    topSearches = withPercentages,
                    limit
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar as principais pesquisas");
                throw new InvalidOperationException("Erro ao buscar as principais pesquisas", ex);
            }
        }

        /// <summary>
        /// Obtain performance statistics
        /// </summary>
        public async Task<object> GetPerformanceStatsAsync()
        {
            try
            {
                var avgQuery = $"movie_search_duration_seconds_sum_{_currentSession} / movie_search_duration_seconds_count_{_currentSession}";
                var p95Query = $"histogram_quantile(0.95, movie_search_duration_seconds_{_currentSession})";

                var avgResult = await QueryAsync(avgQuery);
                var p95Result = await QueryAsync(p95Query);

                var avgDuration = ExtractSingleValue(avgResult) * 1000; // converter para ms
                var p95Duration = ExtractSingleValue(p95Result) * 1000;
                var totalRequests = await GetTotalRequestsAsync();

                return new
                {
                    averageResponseTimeMs = Math.Round(avgDuration, 2),
                    p95ResponseTimeMs = Math.Round(p95Duration, 2),
                    totalRequests
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar estatísticas de performance");
                return new { averageResponseTimeMs = 0, p95ResponseTimeMs = 0, totalRequests = 0 };
            }
        }

        /// <summary>
        /// Get statistics by time of day
        /// </summary>
        public async Task<object> GetHourlyStatsAsync()
        {
            try
            {
                var query = $"sum by (hour) (movie_searches_by_hour_total_{_currentSession})";
                var result = await QueryAsync(query);

                var hourlyStats = ExtractHourlyStats(result);

                // Ordenar por hora
                hourlyStats = hourlyStats.OrderBy(h => h.Hour).ToList();

                var mostActiveHour = hourlyStats.OrderByDescending(h => h.Count).FirstOrDefault();

                result.Dispose();

                return new
                {
                    hourlyDistribution = hourlyStats,
                    mostActiveHour,
                    totalHours = hourlyStats.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when searching for statistics by time");
                throw new InvalidOperationException("Error when searching for statistics by time", ex);
            }
        }

        /// <summary>
        /// Obter estatísticas de status codes
        /// </summary>
        public async Task<object> GetStatusCodeStatsAsync()
        {
            try
            {
                var query = $"sum by (status_code_{_currentSession}) (movie_search_queries_total_{_currentSession})";
                var result = await QueryAsync(query);

                var statusStats = ExtractStatusCodeStats(result);
                var total = statusStats.Sum(s => s.Count);

                var withPercentages = statusStats.Select(s => new
                {
                    statusCode = s.StatusCode,
                    count = s.Count,
                    description = s.Description,
                    category = s.Category,
                    percentage = total > 0 ? Math.Round((double)s.Count / total * 100, 2) : 0
                }).OrderBy(s => s.statusCode).ToList();

                var successRate = withPercentages
                    .Where(s => s.statusCode >= 200 && s.statusCode < 300)
                    .Sum(s => s.percentage);

                result.Dispose();

                return new
                {
                    statusDistribution = withPercentages,
                    totalRequests = total,
                    successRate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when searching for status code statistics");
                throw new InvalidOperationException("Error when searching for status code statistics", ex);
            }
        }


        private async Task<object> GetOverviewStatsAsync()
        {
            try
            {
                var totalSearchesQuery = $"sum(movie_search_queries_total_{_currentSession})";
                var totalByIdQuery = $"sum(movie_by_id_queries_total_{_currentSession})";

                var totalSearchesResult = await QueryAsync(totalSearchesQuery);
                var totalByIdResult = await QueryAsync(totalByIdQuery);

                var totalSearches = ExtractSingleValue(totalSearchesResult);
                var totalById = ExtractSingleValue(totalByIdResult);

                return new
                {
                    totalSearchRequests = (int)totalSearches,
                    totalByIdRequests = (int)totalById,
                    totalRequests = (int)(totalSearches + totalById),
                    avgRequestsPerDay = Math.Round((totalSearches + totalById) / 30, 1)
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error when searching for overview stats, returning default values");
                return new { totalSearchRequests = 0, totalByIdRequests = 0, totalRequests = 0, avgRequestsPerDay = 0.0 };
            }
        }

        private async Task<object> GetUsageStatsAsync()
        {
            try
            {
                var resultRangeQuery = $"sum by (result_count_range_{_currentSession}) (movie_search_results_total_{_currentSession})";
                var result = await QueryAsync(resultRangeQuery);

                var distribution = ExtractResultDistribution(result);

                return new
                {
                    resultDistribution = distribution,
                    mostCommonResultRange = distribution.OrderByDescending(kv => kv.Value).FirstOrDefault().Key ?? "unknown"
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching usage stats, returning default values");
                return new { resultDistribution = new Dictionary<string, int>(), mostCommonResultRange = "unknown" };
            }
        }

        private async Task<int> GetTotalRequestsAsync()
        {
            try
            {
                var query = $"movie_search_duration_seconds_count_{_currentSession}";
                var result = await QueryAsync(query);
                return (int)ExtractSingleValue(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error when searching total requests");
                return 0;
            }
        }

        private List<TopSearchItem> ExtractTopSearches(JsonDocument result)
        {
            var topSearches = new List<TopSearchItem>();

            try
            {
                var root = result.RootElement;
                if (root.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("result", out var results))
                {
                    foreach (var item in results.EnumerateArray())
                    {
                        if (item.TryGetProperty("metric", out var metric) &&
                            item.TryGetProperty("value", out var value))
                        {
                            var searchQuery = metric.TryGetProperty("query", out var q) ? q.GetString() : "unknown";
                            if (value.GetArrayLength() > 1 && double.TryParse(value[1].GetString(), out var count))
                            {
                                topSearches.Add(new TopSearchItem
                                {
                                    Query = searchQuery,
                                    Count = (int)count
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao extrair top buscas");
            }

            return topSearches;
        }

        private List<HourlyStatItem> ExtractHourlyStats(JsonDocument result)
        {
            var hourlyStats = new List<HourlyStatItem>();

            try
            {
                var root = result.RootElement;
                if (root.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("result", out var results))
                {
                    foreach (var item in results.EnumerateArray())
                    {
                        if (item.TryGetProperty("metric", out var metric) &&
                            item.TryGetProperty("value", out var value))
                        {
                            var hourStr = metric.TryGetProperty("hour", out var h) ? h.GetString() : "0";
                            if (value.GetArrayLength() > 1 &&
                                double.TryParse(value[1].GetString(), out var count) &&
                                int.TryParse(hourStr, out var hour))
                            {
                                hourlyStats.Add(new HourlyStatItem
                                {
                                    Hour = hour,
                                    HourDisplay = $"{hour:00}:00",
                                    Count = (int)count,
                                    Period = GetPeriodOfDay(hour)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when extracting statistics by time");
            }

            return hourlyStats;
        }

        private List<StatusCodeStatItem> ExtractStatusCodeStats(JsonDocument result)
        {
            var statusStats = new List<StatusCodeStatItem>();

            try
            {
                var root = result.RootElement;
                if (root.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("result", out var results))
                {
                    foreach (var item in results.EnumerateArray())
                    {
                        if (item.TryGetProperty("metric", out var metric) &&
                            item.TryGetProperty("value", out var value))
                        {
                            var statusStr = metric.TryGetProperty("status_code", out var s) ? s.GetString() : "0";
                            if (value.GetArrayLength() > 1 &&
                                double.TryParse(value[1].GetString(), out var count) &&
                                int.TryParse(statusStr, out var statusCode))
                            {
                                statusStats.Add(new StatusCodeStatItem
                                {
                                    StatusCode = statusCode,
                                    Count = (int)count,
                                    Description = GetStatusCodeDescription(statusCode),
                                    Category = GetStatusCodeCategory(statusCode)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when extracting status code statistics");
            }

            return statusStats;
        }

        private Dictionary<string, int> ExtractResultDistribution(JsonDocument result)
        {
            var distribution = new Dictionary<string, int>();

            try
            {
                var root = result.RootElement;
                if (root.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("result", out var results))
                {
                    foreach (var item in results.EnumerateArray())
                    {
                        if (item.TryGetProperty("metric", out var metric) &&
                            item.TryGetProperty("value", out var value))
                        {
                            var range = metric.TryGetProperty("result_count_range", out var r) ? r.GetString() : "unknown";
                            if (value.GetArrayLength() > 1 && double.TryParse(value[1].GetString(), out var count))
                            {
                                distribution[range] = (int)count;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when extracting result distribution");
            }

            return distribution;
        }

        private double ExtractSingleValue(JsonDocument result)
        {
            try
            {
                var root = result.RootElement;
                if (root.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("result", out var results) &&
                    results.GetArrayLength() > 0)
                {
                    var firstResult = results[0];
                    if (firstResult.TryGetProperty("value", out var value) &&
                        value.GetArrayLength() > 1)
                    {
                        var valueStr = value[1].GetString();
                        if (double.TryParse(valueStr, out var val))
                        {
                            return val;
                        }
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when extracting single value");
                return 0;
            }
        }

        private string GetPeriodOfDay(int hour)
        {
            return hour switch
            {
                >= 6 and < 12 => "Morning",
                >= 12 and < 18 => "Afternoon",
                >= 18 and < 24 => "Evening",
                _ => "Early morning"
            };
        }

        private string GetStatusCodeDescription(int statusCode)
        {
            return statusCode switch
            {
                200 => "Success",
                400 => "Invalid request",
                404 => "Not found",
                500 => "Internal Error",
                _ => "Others"
            };
        }

        private string GetStatusCodeCategory(int statusCode)
        {
            return statusCode switch
            {
                >= 200 and < 300 => "Success",
                >= 400 and < 500 => "Client Error",
                >= 500 => "Server Error",
                _ => "Unknown"
            };
        }


        private string GetResultCountRange(int count)
        {
            return count switch
            {
                0 => "0",
                1 => "1",
                >= 2 and <= 5 => "2-5",
                >= 6 and <= 10 => "6-10",
                _ => "10+"
            };
        }
    }
}