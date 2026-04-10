namespace SuiviSeriesTV.ViewModels;

public class WatchlistReorderRequest
{
    public IReadOnlyList<int> OrderedIds { get; set; } = [];
}
