namespace SuiviSeriesTV.ViewModels.Tmdb;

public class TmdbSearchResultViewModel
{
    public int TmdbId { get; set; }
    public string MediaType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Overview { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public string? BackdropUrl { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public double? VoteAverage { get; set; }
}
