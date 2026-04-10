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
    int EstimateRemainingMinutes(Serie item);
}
