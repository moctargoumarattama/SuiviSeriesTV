using SuiviSeriesTV.Models;

namespace SuiviSeriesTV.ViewModels.User;

public class UserDashboardViewModel
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime MemberSinceUtc { get; set; }

    public int TotalSeries { get; set; }
    public int CompletedSeries { get; set; }
    public int InProgressSeries { get; set; }
    public int ToWatchSeries { get; set; }
    public int FavoriteItems { get; set; }
    public double AverageRating { get; set; }
    public int EstimatedRemainingHours { get; set; }
    public double CompletionRate { get; set; }
    public string ProfileTier { get; set; } = "Explorateur";

    public IReadOnlyList<Serie> RecentSeries { get; set; } = [];
    public IReadOnlyList<Serie> FavoriteSeries { get; set; } = [];
    public IReadOnlyList<UserGenreStatViewModel> TopGenres { get; set; } = [];
    public IReadOnlyList<UserBadgeViewModel> Badges { get; set; } = [];

    public IReadOnlyList<string> WeeklyLabels { get; set; } = [];
    public IReadOnlyList<int> WeeklyActivityValues { get; set; } = [];

    public IReadOnlyList<string> ContentMixLabels { get; set; } = [];
    public IReadOnlyList<int> ContentMixValues { get; set; } = [];
}

public class UserGenreStatViewModel
{
    public string Genre { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class UserBadgeViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconKey { get; set; } = string.Empty;
    public bool Unlocked { get; set; }
}
