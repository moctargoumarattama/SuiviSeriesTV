namespace SuiviSeriesTV.ViewModels;

public class SearchSuggestionViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
}
