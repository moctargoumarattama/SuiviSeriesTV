using SuiviSeriesTV.Models;

namespace SuiviSeriesTV.ViewModels;

public class SeriesQueryOptions
{
    public string? SearchTerm { get; set; }
    public string? Genre { get; set; }
    public SerieStatus? Status { get; set; }
    public ContentType? ContentType { get; set; }
    public bool FavoritesOnly { get; set; }
    public string SortBy { get; set; } = "date_desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
