using SuiviSeriesTV.Models;

namespace SuiviSeriesTV.ViewModels.User;

public class UserDashboardViewModel
{
    public int TotalSeries { get; set; }
    public int CompletedSeries { get; set; }
    public int InProgressSeries { get; set; }
    public int ToWatchSeries { get; set; }
    public double AverageRating { get; set; }
    public IReadOnlyList<Serie> RecentSeries { get; set; } = [];
}
