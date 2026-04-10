namespace SuiviSeriesTV.Helpers;

public static class GenreParser
{
    public static IEnumerable<string> SplitGenres(string? genreValue)
    {
        if (string.IsNullOrWhiteSpace(genreValue))
        {
            return [];
        }

        return genreValue
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(g => !string.IsNullOrWhiteSpace(g));
    }
}

