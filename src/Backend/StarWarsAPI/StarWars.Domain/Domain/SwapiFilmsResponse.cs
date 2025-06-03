namespace StarWarsApi.Domain;

public class SwapiFilmsResponse
{
    public string Message { get; set; } = string.Empty;

    public List<FilmExpanded> Result { get; set; } = new List<FilmExpanded>();

    public DateTime Timestamp { get; set; }

}