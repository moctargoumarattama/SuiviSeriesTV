using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuiviSeriesTV.Constants;
using SuiviSeriesTV.Data;
using SuiviSeriesTV.Helpers;
using SuiviSeriesTV.Models;
using SuiviSeriesTV.Services.Library;
using SuiviSeriesTV.ViewModels;

namespace SuiviSeriesTV.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILibraryService _libraryService;

    public HomeController(ApplicationDbContext context, ILibraryService libraryService)
    {
        _context = context;
        _libraryService = libraryService;
    }

    public async Task<IActionResult> Index()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAuthenticated = User.Identity?.IsAuthenticated == true;
        var isAdmin = User.IsInRole(AppRoles.Admin);

        var scopedQuery = _context.Series.AsNoTracking();
        if (isAuthenticated && !isAdmin && !string.IsNullOrWhiteSpace(currentUserId))
        {
            scopedQuery = scopedQuery.Where(s => s.OwnerId == currentUserId);
        }

        var activeQuery = scopedQuery
            .Where(s => s.Status != SerieStatus.Abandonne)
            .Select(s => new Serie
            {
                Id = s.Id,
                Title = s.Title,
                Genre = s.Genre,
                ContentType = s.ContentType,
                Status = s.Status,
                PersonalRating = s.PersonalRating,
                StreamingPlatform = s.StreamingPlatform,
                DateAdded = s.DateAdded,
                ReleaseDate = s.ReleaseDate,
                PosterUrl = s.PosterUrl,
                BackdropUrl = s.BackdropUrl,
                WatchedEpisodes = s.WatchedEpisodes,
                TotalEpisodes = s.TotalEpisodes,
                IsFavorite = s.IsFavorite
            });

        var topTenToday = await activeQuery
            .OrderByDescending(s => s.PersonalRating)
            .ThenByDescending(s => s.IsFavorite)
            .ThenByDescending(s => s.DateAdded)
            .ThenBy(s => s.Title)
            .Take(10)
            .ToListAsync();

        var trendingMovies = topTenToday.Take(6).ToList();

        var becauseYouLikedMovies = new List<Serie>();
        string becauseYouLikedTitle = "Parce que vous avez aime";
        if (isAuthenticated && !isAdmin && !string.IsNullOrWhiteSpace(currentUserId))
        {
            var userGenres = await scopedQuery
                .Where(s => !string.IsNullOrWhiteSpace(s.Genre))
                .Select(s => s.Genre)
                .ToListAsync();

            var favoriteGenres = userGenres
                .SelectMany(GenreParser.SplitGenres)
                .GroupBy(g => g, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .Select(g => g.Key)
                .Take(2)
                .ToList();

            if (favoriteGenres.Any())
            {
                becauseYouLikedTitle = $"Parce que vous avez aime {favoriteGenres[0]}";
            }

            var candidates = await activeQuery
                .OrderByDescending(s => s.IsFavorite)
                .ThenByDescending(s => s.PersonalRating)
                .ThenByDescending(s => s.DateAdded)
                .ThenBy(s => s.Title)
                .Take(60)
                .ToListAsync();

            var personalized = candidates
                .Where(s => s.Status == SerieStatus.Watchlist || s.Status == SerieStatus.EnCours)
                .Take(6)
                .ToList();

            becauseYouLikedMovies.AddRange(personalized);

            if (becauseYouLikedMovies.Count < 6 && favoriteGenres.Any())
            {
                var favoriteGenreSet = favoriteGenres.ToHashSet(StringComparer.OrdinalIgnoreCase);
                var addedIds = becauseYouLikedMovies.Select(x => x.Id).ToHashSet();

                var byGenres = candidates
                    .Where(s => !addedIds.Contains(s.Id) && MatchesAnyGenre(s, favoriteGenreSet))
                    .Take(6 - becauseYouLikedMovies.Count)
                    .ToList();

                becauseYouLikedMovies.AddRange(byGenres);
            }
        }
        else
        {
            becauseYouLikedMovies = await activeQuery
                .OrderByDescending(s => s.DateAdded)
                .ThenByDescending(s => s.PersonalRating)
                .Take(6)
                .ToListAsync();
        }

        if (!becauseYouLikedMovies.Any())
        {
            becauseYouLikedMovies = trendingMovies.ToList();
        }

        var newReleaseMovies = await activeQuery
            .OrderByDescending(s => s.ReleaseDate ?? s.DateAdded)
            .ThenByDescending(s => s.DateAdded)
            .ThenBy(s => s.Title)
            .Take(6)
            .ToListAsync();

        IReadOnlyList<Serie> continueWatching = !isAuthenticated || string.IsNullOrWhiteSpace(currentUserId)
            ? Array.Empty<Serie>()
            : await activeQuery
                .Where(s => s.Status == SerieStatus.EnCours && s.WatchedEpisodes > 0 && s.WatchedEpisodes < s.TotalEpisodes)
                .OrderByDescending(s => s.DateAdded)
                .ThenByDescending(s => s.PersonalRating)
                .Take(8)
                .ToListAsync();

        var allCatalogItems = await activeQuery
            .OrderByDescending(s => s.PersonalRating)
            .ThenByDescending(s => s.DateAdded)
            .Take(400)
            .ToListAsync();

        var model = new HomeIndexViewModel
        {
            IsAuthenticated = isAuthenticated,
            TrendingMovies = trendingMovies,
            TopTenToday = topTenToday,
            BecauseYouLikedMovies = becauseYouLikedMovies,
            BecauseYouLikedTitle = becauseYouLikedTitle,
            NewReleaseMovies = newReleaseMovies,
            ContinueWatching = continueWatching,
            GenreCards = BuildGenreCards(allCatalogItems)
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> SearchSuggestions(string? query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
        {
            return Json(Array.Empty<object>());
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(AppRoles.Admin);
        var normalizedQuery = query.Trim();

        IReadOnlyList<SearchSuggestionViewModel> suggestions;
        if (!string.IsNullOrWhiteSpace(currentUserId) || isAdmin)
        {
            suggestions = await _libraryService.GetSearchSuggestionsAsync(currentUserId ?? string.Empty, isAdmin, normalizedQuery, 8);
        }
        else
        {
            suggestions = await _libraryService.GetPublicSearchSuggestionsAsync(normalizedQuery, 8);
        }

        return Json(suggestions);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static IReadOnlyList<HomeGenreCardViewModel> BuildGenreCards(IReadOnlyList<Serie> items)
    {
        var cards = items
            .Where(f => !string.IsNullOrWhiteSpace(f.Genre))
            .SelectMany(item => GenreParser.SplitGenres(item.Genre).Select(genre => new { Genre = genre, Item = item }))
            .GroupBy(x => x.Genre, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Select(g => new HomeGenreCardViewModel
            {
                Genre = g.Key,
                ItemCount = g.Count(),
                CoverUrl = g.Select(x => !string.IsNullOrWhiteSpace(x.Item.BackdropUrl) ? x.Item.BackdropUrl : x.Item.PosterUrl)
                    .FirstOrDefault(url => !string.IsNullOrWhiteSpace(url))
                    ?? string.Empty
            })
            .ToList();

        var usedGenres = new HashSet<string>(cards.Select(c => c.Genre), StringComparer.OrdinalIgnoreCase);
        foreach (var fallback in GetFallbackGenreCards())
        {
            if (usedGenres.Add(fallback.Genre))
            {
                cards.Add(fallback);
            }
        }

        return cards
            .Where(c => !string.IsNullOrWhiteSpace(c.CoverUrl))
            .Take(24)
            .ToList();
    }

    private static bool MatchesAnyGenre(Serie item, HashSet<string> favoriteGenres)
    {
        foreach (var genre in GenreParser.SplitGenres(item.Genre))
        {
            if (favoriteGenres.Contains(genre))
            {
                return true;
            }
        }

        return false;
    }

    private static IReadOnlyList<HomeGenreCardViewModel> GetFallbackGenreCards()
    {
        return
        [
            new() { Genre = "Action", CoverUrl = "https://image.tmdb.org/t/p/w780/2u7zbn8EudG6kLlBzUYqP8RyFU4.jpg" },
            new() { Genre = "Aventure", CoverUrl = "https://image.tmdb.org/t/p/w780/8Y43POKjjKDGI9MH89NW0NAzzp8.jpg" },
            new() { Genre = "Animation", CoverUrl = "https://image.tmdb.org/t/p/w780/hU42CRk14JuPEdqZG3AWmagiPAP.jpg" },
            new() { Genre = "Anime", CoverUrl = "https://image.tmdb.org/t/p/w780/mMtUybQ6hL24FXo0F3Z4j2KG7kZ.jpg" },
            new() { Genre = "Comedie", CoverUrl = "https://image.tmdb.org/t/p/w780/8uO0gUM8aNqYLs1OsTBQiXu0fEv.jpg" },
            new() { Genre = "Crime", CoverUrl = "https://image.tmdb.org/t/p/w780/umC04Cozevu8nn3JTDJ1pc7PVTn.jpg" },
            new() { Genre = "Documentaire", CoverUrl = "https://image.tmdb.org/t/p/w780/i6BsWXxUSHp7f3f2qte8nTjQqA0.jpg" },
            new() { Genre = "Drame", CoverUrl = "https://image.tmdb.org/t/p/w780/rSPw7tgCH9c6NqICZef4kZjFOQ5.jpg" },
            new() { Genre = "Fantastique", CoverUrl = "https://image.tmdb.org/t/p/w780/xJHokMbljvjADYdit5fK5VQsXEG.jpg" },
            new() { Genre = "Horreur", CoverUrl = "https://image.tmdb.org/t/p/w780/52AfXWuXCHn3UjD17rBruA9f5qb.jpg" },
            new() { Genre = "Romance", CoverUrl = "https://image.tmdb.org/t/p/w780/5xUJfzPZ8jWJUDzYtIeuPO4qPIa.jpg" },
            new() { Genre = "Science-fiction", CoverUrl = "https://image.tmdb.org/t/p/w780/xOMo8BRK7PfcJv9JCnx7s5hj0PX.jpg" },
            new() { Genre = "Thriller", CoverUrl = "https://image.tmdb.org/t/p/w780/euCnMxNRlHNxA4f9BMnWbmxPOse.jpg" },
            new() { Genre = "Mystere", CoverUrl = "https://image.tmdb.org/t/p/w780/6LxMopY4rwW4qQ1Lk7xQx36JH7j.jpg" }
        ];
    }
}

