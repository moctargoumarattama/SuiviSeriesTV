namespace SuiviSeriesTV.ViewModels.Tmdb;

public class TmdbSearchPageViewModel
{
    public string Query { get; set; } = string.Empty;
    public bool TmdbEnabled { get; set; }
    public string? ErrorMessage { get; set; }
    public IReadOnlyList<TmdbSearchResultViewModel> Results { get; set; } = [];
}
