using SuiviSeriesTV.Models;

namespace SuiviSeriesTV.ViewModels;

public class SeriesIndexViewModel
{
    public IReadOnlyList<Serie> Series { get; set; } = [];
    public IReadOnlyList<string> Genres { get; set; } = [];
    public IReadOnlyList<string> SortOptions { get; set; } =
    [
        "date_desc",
        "date_asc",
        "title_asc",
        "title_desc",
        "rating_desc",
        "progress_desc"
    ];

    public string? SearchTerm { get; set; }
    public string? Genre { get; set; }
    public SerieStatus? Status { get; set; }
    public ContentType? ContentType { get; set; }
    public bool FavoritesOnly { get; set; }
    public string SortBy { get; set; } = "date_desc";

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalItems { get; set; }
    public int TotalPages => TotalItems <= 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public int WatchlistCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int FavoritesCount { get; set; }

    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public string? BuildPageUrl(int targetPage)
    {
        var clampedPage = Math.Clamp(targetPage, 1, Math.Max(1, TotalPages));
        return $"/Series?searchTerm={Uri.EscapeDataString(SearchTerm ?? string.Empty)}" +
               $"&genre={Uri.EscapeDataString(Genre ?? string.Empty)}" +
               $"&status={(Status.HasValue ? (int)Status.Value : string.Empty)}" +
               $"&contentType={(ContentType.HasValue ? (int)ContentType.Value : string.Empty)}" +
               $"&favoritesOnly={FavoritesOnly}" +
               $"&sortBy={Uri.EscapeDataString(SortBy)}" +
               $"&page={clampedPage}";
    }
}
