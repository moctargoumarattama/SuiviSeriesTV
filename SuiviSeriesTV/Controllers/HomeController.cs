using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuiviSeriesTV.Data;
using SuiviSeriesTV.Models;
using SuiviSeriesTV.ViewModels;

namespace SuiviSeriesTV.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var filmsQuery = _context.Series
            .AsNoTracking()
            .Where(s => s.ContentType == ContentType.Film);

        var trendingMovies = await filmsQuery
            .OrderByDescending(s => s.PersonalRating)
            .ThenByDescending(s => s.DateAdded)
            .ThenBy(s => s.Title)
            .Take(6)
            .ToListAsync();

        IReadOnlyList<Serie> recommendedMovies;
        if (!string.IsNullOrWhiteSpace(currentUserId))
        {
            var personnalized = await _context.Series
                .AsNoTracking()
                .Where(s =>
                    s.OwnerId == currentUserId &&
                    s.ContentType == ContentType.Film &&
                    (s.Status == SerieStatus.Watchlist || s.Status == SerieStatus.EnCours))
                .OrderByDescending(s => s.IsFavorite)
                .ThenByDescending(s => s.PersonalRating)
                .ThenByDescending(s => s.DateAdded)
                .Take(6)
                .ToListAsync();

            if (personnalized.Any())
            {
                recommendedMovies = personnalized;
            }
            else
            {
                var favoriteGenres = await _context.Series
                    .AsNoTracking()
                    .Where(s => s.OwnerId == currentUserId && !string.IsNullOrWhiteSpace(s.Genre))
                    .GroupBy(s => s.Genre)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .Take(3)
                    .ToListAsync();

                recommendedMovies = favoriteGenres.Any()
                    ? await filmsQuery
                        .Where(s => favoriteGenres.Contains(s.Genre))
                        .OrderByDescending(s => s.PersonalRating)
                        .ThenByDescending(s => s.DateAdded)
                        .Take(6)
                        .ToListAsync()
                    : trendingMovies;
            }
        }
        else
        {
            recommendedMovies = await filmsQuery
                .Where(s => s.Status != SerieStatus.Abandonne)
                .OrderByDescending(s => s.DateAdded)
                .ThenByDescending(s => s.PersonalRating)
                .Take(6)
                .ToListAsync();
        }

        if (!recommendedMovies.Any())
        {
            recommendedMovies = trendingMovies;
        }

        var allFilms = await filmsQuery
            .OrderByDescending(s => s.PersonalRating)
            .ThenByDescending(s => s.DateAdded)
            .ToListAsync();

        var model = new HomeIndexViewModel
        {
            IsAuthenticated = User.Identity?.IsAuthenticated == true,
            TrendingMovies = trendingMovies,
            RecommendedMovies = recommendedMovies,
            GenreCards = BuildGenreCards(allFilms)
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static IReadOnlyList<HomeGenreCardViewModel> BuildGenreCards(IReadOnlyList<Serie> films)
    {
        var cards = films
            .Where(f => !string.IsNullOrWhiteSpace(f.Genre))
            .GroupBy(f => f.Genre.Trim(), StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Select(g => new HomeGenreCardViewModel
            {
                Genre = g.First().Genre,
                ItemCount = g.Count(),
                CoverUrl = g.Select(x => !string.IsNullOrWhiteSpace(x.BackdropUrl) ? x.BackdropUrl : x.PosterUrl)
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

