using SuiviSeriesTV.Models;
using SuiviSeriesTV.ViewModels;

namespace SuiviSeriesTV.Services.Library;

public interface ILibraryService
{
    Task<SeriesIndexViewModel> GetLibraryAsync(string userId, bool isAdmin, SeriesQueryOptions options);
    Task<SeriesDashboardViewModel> GetDashboardAsync(string userId, bool isAdmin);
    Task<Serie?> GetAccessibleByIdAsync(int id, string userId, bool isAdmin);
    Task<bool> MarkAsWatchedAsync(int id, string userId, bool isAdmin);
    Task<bool> ToggleFavoriteAsync(int id, string userId, bool isAdmin);
    Task<bool> ReorderWatchlistAsync(string userId, bool isAdmin, IReadOnlyList<int> orderedIds);
    Task<IReadOnlyList<SearchSuggestionViewModel>> GetSearchSuggestionsAsync(string userId, bool isAdmin, string query, int limit = 6);
    Task<IReadOnlyList<SearchSuggestionViewModel>> GetPublicSearchSuggestionsAsync(string query, int limit = 6);
    int EstimateRemainingMinutes(Serie item);
}
