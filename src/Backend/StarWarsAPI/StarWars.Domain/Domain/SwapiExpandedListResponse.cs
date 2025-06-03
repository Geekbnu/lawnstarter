using StarWars.Domain.Domain;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StarWarsApi.Domain;

public class SwapiExpandedListResponse
{
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("total_records")]
    public int TotalRecords { get; set; }

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }

    public string? Previous { get; set; }

    public string? Next { get; set; }

    public List<PersonExpanded> Results { get; set; } = new List<PersonExpanded>();

    public DateTime Timestamp { get; set; }


}
