namespace SuiviSeriesTV.Configuration;

public class TmdbSettings
{
    public const string SectionName = "Tmdb";

    public bool Enabled { get; set; } = false;
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.themoviedb.org/3";
    public string ImageBaseUrl { get; set; } = "https://image.tmdb.org/t/p/w500";
}
