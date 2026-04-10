namespace SuiviSeriesTV.ViewModels.Tmdb;

public class TmdbMediaDetailsViewModel
{
    public string? TrailerUrl { get; set; }
    public string? TrailerName { get; set; }
    public IReadOnlyList<string> Cast { get; set; } = [];
    public IReadOnlyList<string> Genres { get; set; } = [];
    public int? RuntimeMinutes { get; set; }
}
