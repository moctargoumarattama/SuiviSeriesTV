using SuiviSeriesTV.Models;
using SuiviSeriesTV.ViewModels.Tmdb;

namespace SuiviSeriesTV.ViewModels;

public class SeriesDetailsViewModel
{
    public Serie Item { get; set; } = new();
    public TmdbMediaDetailsViewModel? Tmdb { get; set; }
}
