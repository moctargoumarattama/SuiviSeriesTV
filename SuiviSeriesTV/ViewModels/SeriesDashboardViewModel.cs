using SuiviSeriesTV.Models;

namespace SuiviSeriesTV.ViewModels;

public class SeriesDashboardViewModel
{
    public int TotalItems { get; set; }
    public int CompletedItems { get; set; }
    public int InProgressItems { get; set; }
    public int WatchlistItems { get; set; }
    public int FavoriteItems { get; set; }
    public double AverageRating { get; set; }
    public int EstimatedRemainingMinutes { get; set; }

    public IReadOnlyList<Serie> WatchNext { get; set; } = [];
    public IReadOnlyList<Serie> ResumeItems { get; set; } = [];
    public IReadOnlyList<Serie> UpcomingThisWeek { get; set; } = [];

    public IReadOnlyList<string> ChartLabels { get; set; } = [];
    public IReadOnlyList<int> ChartValues { get; set; } = [];
}
