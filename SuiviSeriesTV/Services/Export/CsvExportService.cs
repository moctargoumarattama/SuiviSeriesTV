using System.Text;
using Microsoft.EntityFrameworkCore;
using SuiviSeriesTV.Data;

namespace SuiviSeriesTV.Services.Export;

public class CsvExportService : IExportService
{
    private readonly ApplicationDbContext _context;

    public CsvExportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> BuildLibraryCsvAsync(string userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var query = _context.Series.AsNoTracking();
        if (!isAdmin)
        {
            query = query.Where(s => s.OwnerId == userId);
        }

        var items = await query.OrderByDescending(s => s.DateAdded).ThenBy(s => s.Title).ToListAsync(cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine("Type;Titre;Genre;Statut;EpisodesVus;TotalEpisodes;Progression;Note;Favori;DateAjout;ProchaineSortie;Plateforme");

        foreach (var item in items)
        {
            sb.AppendLine(string.Join(";", [
                Escape(item.ContentType.ToString()),
                Escape(item.Title),
                Escape(item.Genre),
                Escape(item.Status.ToString()),
                item.WatchedEpisodes.ToString(),
                item.TotalEpisodes.ToString(),
                item.ProgressPercentage.ToString(),
                item.PersonalRating.ToString("0.0"),
                item.IsFavorite ? "Oui" : "Non",
                item.DateAdded.ToString("yyyy-MM-dd"),
                item.NextReleaseDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                Escape(item.StreamingPlatform)
            ]));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string Escape(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var normalized = input.Replace("\"", "\"\"");
        return $"\"{normalized}\"";
    }
}
