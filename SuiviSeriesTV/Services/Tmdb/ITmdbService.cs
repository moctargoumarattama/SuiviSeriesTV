using SuiviSeriesTV.Models;
using SuiviSeriesTV.ViewModels.Tmdb;

namespace SuiviSeriesTV.Services.Tmdb;

public interface ITmdbService
{
    bool IsEnabled { get; }
    Task<IReadOnlyList<TmdbSearchResultViewModel>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task<Serie?> BuildItemFromTmdbAsync(string mediaType, int tmdbId, CancellationToken cancellationToken = default);
    Task<TmdbMediaDetailsViewModel?> GetMediaDetailsAsync(string mediaType, int tmdbId, CancellationToken cancellationToken = default);
}
