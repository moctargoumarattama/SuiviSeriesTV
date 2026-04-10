using SuiviSeriesTV.Models;

namespace SuiviSeriesTV.ViewModels;

public class HomeIndexViewModel
{
    public bool IsAuthenticated { get; set; }
    public IReadOnlyList<Serie> TrendingMovies { get; set; } = [];
    public IReadOnlyList<Serie> RecommendedMovies { get; set; } = [];
    public IReadOnlyList<HomeGenreCardViewModel> GenreCards { get; set; } = [];
}

public class HomeGenreCardViewModel
{
    public string Genre { get; set; } = string.Empty;
    public string CoverUrl { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

