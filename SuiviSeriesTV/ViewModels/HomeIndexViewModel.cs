using SuiviSeriesTV.Models;

namespace SuiviSeriesTV.ViewModels;

public class HomeIndexViewModel
{
    public bool IsAuthenticated { get; set; }
    public IReadOnlyList<Serie> TrendingMovies { get; set; } = [];
    public IReadOnlyList<Serie> TopTenToday { get; set; } = [];
    public IReadOnlyList<Serie> BecauseYouLikedMovies { get; set; } = [];
    public IReadOnlyList<Serie> NewReleaseMovies { get; set; } = [];
    public IReadOnlyList<Serie> ContinueWatching { get; set; } = [];
    public string BecauseYouLikedTitle { get; set; } = "Parce que vous avez aime";
    public IReadOnlyList<HomeGenreCardViewModel> GenreCards { get; set; } = [];
}

public class HomeGenreCardViewModel
{
    public string Genre { get; set; } = string.Empty;
    public string CoverUrl { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

