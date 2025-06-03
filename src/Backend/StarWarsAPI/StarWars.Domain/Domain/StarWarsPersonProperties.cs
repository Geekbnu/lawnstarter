using System;
using System.Text.Json.Serialization;

namespace StarWarsApi.Domain;

public class StarWarsPersonProperties
{
    public DateTime Created { get; set; }

    public DateTime Edited { get; set; }

    public string Height { get; set; } = string.Empty;

    public string Mass { get; set; } = string.Empty;

    [JsonPropertyName("hair_color")]
    public string HairColor { get; set; } = string.Empty;

    [JsonPropertyName("skin_color")]
    public string SkinColor { get; set; } = string.Empty;

    [JsonPropertyName("eye_color")]
    public string EyeColor { get; set; } = string.Empty;

    [JsonPropertyName("birth_year")]
    public string BirthYear { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Homeworld { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
}
