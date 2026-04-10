using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Globalization;
using SuiviSeriesTV.Data;
using SuiviSeriesTV.Helpers;
using SuiviSeriesTV.Models;
using SuiviSeriesTV.Services.Library;
using SuiviSeriesTV.ViewModels.User;

namespace SuiviSeriesTV.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILibraryService _libraryService;

    public UserController(ApplicationDbContext context, ILibraryService libraryService)
    {
        _context = context;
        _libraryService = libraryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            return Challenge();
        }

        var query = _context.Series
            .AsNoTracking()
            .Where(s => s.OwnerId == userId);

        var items = await query
            .OrderByDescending(s => s.DateAdded)
            .ToListAsync();

        var total = items.Count;
        var completed = items.Count(s => s.Status == SerieStatus.Termine);
        var inProgress = items.Count(s => s.Status == SerieStatus.EnCours);
        var watchlist = items.Count(s => s.Status == SerieStatus.Watchlist);
        var favorites = items.Count(s => s.IsFavorite);
        var avg = items.Count == 0 ? 0 : items.Average(s => s.PersonalRating);
        var remainingMinutes = items.Sum(_libraryService.EstimateRemainingMinutes);
        var estimatedRemainingHours = (int)Math.Ceiling(remainingMinutes / 60d);
        var completionRate = total <= 0 ? 0 : Math.Round((double)completed * 100 / total, 1);
        var profileTier = ResolveProfileTier(total, completionRate, avg);

        var topGenres = items
            .SelectMany(x => GenreParser.SplitGenres(x.Genre))
            .GroupBy(g => g, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Take(5)
            .Select(g => new UserGenreStatViewModel
            {
                Genre = g.Key,
                Count = g.Count()
            })
            .ToList();

        var today = DateTime.UtcNow.Date;
        var dates = Enumerable.Range(0, 7)
            .Select(offset => today.AddDays(-(6 - offset)))
            .ToList();

        var weeklyMap = items
            .GroupBy(s => s.DateAdded.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var weeklyLabels = dates
            .Select(d => d.ToString("ddd", CultureInfo.GetCultureInfo("fr-FR")))
            .ToList();

        var weeklyActivityValues = dates
            .Select(d => weeklyMap.TryGetValue(d, out var count) ? count : 0)
            .ToList();

        var contentMix = items
            .GroupBy(s => s.ContentType)
            .OrderBy(g => g.Key)
            .Select(g => new { Label = g.Key.ToString(), Count = g.Count() })
            .ToList();

        var badges = BuildBadges(total, completed, inProgress, watchlist, favorites, avg, completionRate);

        var model = new UserDashboardViewModel
        {
            DisplayName = string.IsNullOrWhiteSpace(user.UserName) ? "Utilisateur" : user.UserName,
            Email = user.Email ?? string.Empty,
            MemberSinceUtc = user.CreatedAtUtc,
            TotalSeries = total,
            CompletedSeries = completed,
            InProgressSeries = inProgress,
            ToWatchSeries = watchlist,
            FavoriteItems = favorites,
            AverageRating = Math.Round(avg, 2),
            EstimatedRemainingHours = estimatedRemainingHours,
            CompletionRate = completionRate,
            ProfileTier = profileTier,
            RecentSeries = items.Take(8).ToList(),
            FavoriteSeries = items
                .Where(s => s.IsFavorite)
                .OrderByDescending(s => s.PersonalRating)
                .ThenByDescending(s => s.DateAdded)
                .Take(6)
                .ToList(),
            TopGenres = topGenres,
            Badges = badges,
            WeeklyLabels = weeklyLabels,
            WeeklyActivityValues = weeklyActivityValues,
            ContentMixLabels = contentMix.Select(x => x.Label).ToList(),
            ContentMixValues = contentMix.Select(x => x.Count).ToList()
        };

        return View(model);
    }

    private static string ResolveProfileTier(int total, double completionRate, double avgRating)
    {
        if (total >= 50 && completionRate >= 45 && avgRating >= 8.2)
        {
            return "Maitre du suivi";
        }

        if (total >= 25 && completionRate >= 35 && avgRating >= 7.8)
        {
            return "Cinephile confirme";
        }

        if (total >= 10)
        {
            return "Collectionneur";
        }

        return "Explorateur";
    }

    private static IReadOnlyList<UserBadgeViewModel> BuildBadges(
        int total,
        int completed,
        int inProgress,
        int watchlist,
        int favorites,
        double averageRating,
        double completionRate)
    {
        return
        [
            new UserBadgeViewModel
            {
                Title = "Premier pas",
                Description = "Ajouter au moins 1 contenu",
                IconKey = "spark",
                Unlocked = total >= 1
            },
            new UserBadgeViewModel
            {
                Title = "Collection 20",
                Description = "Construire une bibliotheque de 20 contenus",
                IconKey = "stack",
                Unlocked = total >= 20
            },
            new UserBadgeViewModel
            {
                Title = "Marathon",
                Description = "Avoir 3 contenus en cours",
                IconKey = "bolt",
                Unlocked = inProgress >= 3
            },
            new UserBadgeViewModel
            {
                Title = "Critique pointu",
                Description = "Garder une moyenne de notes >= 8.0",
                IconKey = "star",
                Unlocked = averageRating >= 8.0
            },
            new UserBadgeViewModel
            {
                Title = "Completiste",
                Description = "Atteindre 40% de completion",
                IconKey = "shield",
                Unlocked = completionRate >= 40
            },
            new UserBadgeViewModel
            {
                Title = "Curateur",
                Description = "Avoir 5 favoris",
                IconKey = "heart",
                Unlocked = favorites >= 5
            },
            new UserBadgeViewModel
            {
                Title = "Visionnaire",
                Description = "Maintenir une watchlist active de 6+ contenus",
                IconKey = "eye",
                Unlocked = watchlist >= 6
            },
            new UserBadgeViewModel
            {
                Title = "Finisseur",
                Description = "Terminer 10 contenus",
                IconKey = "check",
                Unlocked = completed >= 10
            }
        ];
    }
}
