using Microsoft.EntityFrameworkCore;
using SuiviSeriesTV.Data;
using SuiviSeriesTV.Models;
using SuiviSeriesTV.ViewModels;

namespace SuiviSeriesTV.Services.Library;

public class LibraryService : ILibraryService
{
    private readonly ApplicationDbContext _context;

    public LibraryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SeriesIndexViewModel> GetLibraryAsync(string userId, bool isAdmin, SeriesQueryOptions options)
    {
        var page = Math.Max(1, options.Page);
        var pageSize = options.PageSize is <= 0 or > 50 ? 12 : options.PageSize;

        var baseQuery = BuildUserScope(_context.Series.AsNoTracking(), userId, isAdmin);

        var genres = await baseQuery
            .Select(s => s.Genre)
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync();

        var watchlistCount = await baseQuery.CountAsync(s => s.Status == SerieStatus.Watchlist);
        var inProgressCount = await baseQuery.CountAsync(s => s.Status == SerieStatus.EnCours);
        var completedCount = await baseQuery.CountAsync(s => s.Status == SerieStatus.Termine);
        var favoritesCount = await baseQuery.CountAsync(s => s.IsFavorite);

        var filteredQuery = ApplyFilters(baseQuery, options);
        var sortedQuery = ApplySorting(filteredQuery, options.SortBy);

        var totalItems = await sortedQuery.CountAsync();
        var items = await sortedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new SeriesIndexViewModel
        {
            Series = items,
            Genres = genres,
            SearchTerm = options.SearchTerm,
            Genre = options.Genre,
            Status = options.Status,
            ContentType = options.ContentType,
            FavoritesOnly = options.FavoritesOnly,
            SortBy = options.SortBy,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            WatchlistCount = watchlistCount,
            InProgressCount = inProgressCount,
            CompletedCount = completedCount,
            FavoritesCount = favoritesCount
        };
    }

    public async Task<SeriesDashboardViewModel> GetDashboardAsync(string userId, bool isAdmin)
    {
        var query = BuildUserScope(_context.Series.AsNoTracking(), userId, isAdmin);
        var today = DateTime.UtcNow.Date;
        var weekEnd = today.AddDays(7);

        var total = await query.CountAsync();
        var inProgress = await query.CountAsync(s => s.Status == SerieStatus.EnCours);
        var completed = await query.CountAsync(s => s.Status == SerieStatus.Termine);
        var watchlist = await query.CountAsync(s => s.Status == SerieStatus.Watchlist);
        var favorites = await query.CountAsync(s => s.IsFavorite);
        var averageRating = await query.AverageAsync(s => (double?)s.PersonalRating) ?? 0;

        var watchNext = await query
            .Where(s => s.Status == SerieStatus.EnCours || s.Status == SerieStatus.Watchlist)
            .OrderByDescending(s => s.Status == SerieStatus.EnCours)
            .ThenByDescending(s => s.PersonalRating)
            .ThenBy(s => s.DateAdded)
            .Take(6)
            .ToListAsync();

        var resumeItems = await query
            .Where(s => s.Status == SerieStatus.EnCours && s.WatchedEpisodes > 0 && s.WatchedEpisodes < s.TotalEpisodes)
            .OrderByDescending(s => s.DateAdded)
            .Take(6)
            .ToListAsync();

        var upcoming = await query
            .Where(s => s.NextReleaseDate.HasValue && s.NextReleaseDate.Value >= today && s.NextReleaseDate.Value <= weekEnd)
            .OrderBy(s => s.NextReleaseDate)
            .ThenBy(s => s.Title)
            .Take(8)
            .ToListAsync();

        var allItems = await query.ToListAsync();
        var remainingMinutes = allItems.Sum(EstimateRemainingMinutes);

        return new SeriesDashboardViewModel
        {
            TotalItems = total,
            CompletedItems = completed,
            InProgressItems = inProgress,
            WatchlistItems = watchlist,
            FavoriteItems = favorites,
            AverageRating = Math.Round(averageRating, 2),
            EstimatedRemainingMinutes = remainingMinutes,
            WatchNext = watchNext,
            ResumeItems = resumeItems,
            UpcomingThisWeek = upcoming,
            ChartLabels = ["A voir", "En cours", "Termine", "Abandonne"],
            ChartValues =
            [
                watchlist,
                inProgress,
                completed,
                await query.CountAsync(s => s.Status == SerieStatus.Abandonne)
            ]
        };
    }

    public async Task<Serie?> GetAccessibleByIdAsync(int id, string userId, bool isAdmin)
    {
        return await BuildUserScope(_context.Series, userId, isAdmin)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<bool> MarkAsWatchedAsync(int id, string userId, bool isAdmin)
    {
        var item = await GetAccessibleByIdAsync(id, userId, isAdmin);
        if (item is null)
        {
            return false;
        }

        if (item.WatchedEpisodes >= item.TotalEpisodes)
        {
            item.Status = SerieStatus.Termine;
            await _context.SaveChangesAsync();
            return true;
        }

        item.WatchedEpisodes += 1;
        item.LastWatchedEpisode = item.WatchedEpisodes;
        item.LastWatchedSeason = Math.Max(item.LastWatchedSeason, 1);

        item.Status = item.WatchedEpisodes >= item.TotalEpisodes
            ? SerieStatus.Termine
            : SerieStatus.EnCours;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleFavoriteAsync(int id, string userId, bool isAdmin)
    {
        var item = await GetAccessibleByIdAsync(id, userId, isAdmin);
        if (item is null)
        {
            return false;
        }

        item.IsFavorite = !item.IsFavorite;
        await _context.SaveChangesAsync();
        return true;
    }

    public int EstimateRemainingMinutes(Serie item)
    {
        var remainingEpisodes = Math.Max(0, item.TotalEpisodes - item.WatchedEpisodes);
        var runtime = item.AverageEpisodeRuntimeMinutes.GetValueOrDefault(item.ContentType == ContentType.Film ? 120 : 45);
        return remainingEpisodes * runtime;
    }

    private static IQueryable<Serie> BuildUserScope(IQueryable<Serie> query, string userId, bool isAdmin)
    {
        return isAdmin ? query : query.Where(s => s.OwnerId == userId);
    }

    private static IQueryable<Serie> ApplyFilters(IQueryable<Serie> query, SeriesQueryOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.SearchTerm))
        {
            query = query.Where(s =>
                s.Title.Contains(options.SearchTerm) ||
                s.Description.Contains(options.SearchTerm));
        }

        if (!string.IsNullOrWhiteSpace(options.Genre))
        {
            query = query.Where(s => s.Genre == options.Genre);
        }

        if (options.Status.HasValue)
        {
            query = query.Where(s => s.Status == options.Status.Value);
        }

        if (options.ContentType.HasValue)
        {
            query = query.Where(s => s.ContentType == options.ContentType.Value);
        }

        if (options.FavoritesOnly)
        {
            query = query.Where(s => s.IsFavorite);
        }

        return query;
    }

    private static IQueryable<Serie> ApplySorting(IQueryable<Serie> query, string? sortBy)
    {
        return sortBy switch
        {
            "date_asc" => query.OrderBy(s => s.DateAdded).ThenBy(s => s.Title),
            "title_asc" => query.OrderBy(s => s.Title),
            "title_desc" => query.OrderByDescending(s => s.Title),
            "rating_desc" => query.OrderByDescending(s => s.PersonalRating).ThenBy(s => s.Title),
            "progress_desc" => query.OrderByDescending(s => (double)s.WatchedEpisodes / s.TotalEpisodes).ThenBy(s => s.Title),
            _ => query.OrderByDescending(s => s.DateAdded).ThenBy(s => s.Title)
        };
    }
}

