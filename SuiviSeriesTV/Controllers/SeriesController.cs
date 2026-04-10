using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuiviSeriesTV.Constants;
using SuiviSeriesTV.Data;
using SuiviSeriesTV.Models;
using SuiviSeriesTV.Services.Export;
using SuiviSeriesTV.Services.Library;
using SuiviSeriesTV.Services.Storage;
using SuiviSeriesTV.Services.Tmdb;
using SuiviSeriesTV.ViewModels;
using SuiviSeriesTV.ViewModels.Tmdb;

namespace SuiviSeriesTV.Controllers;

[Authorize]
public class SeriesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILibraryService _libraryService;
    private readonly ITmdbService _tmdbService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IExportService _exportService;

    public SeriesController(
        ApplicationDbContext context,
        ILibraryService libraryService,
        ITmdbService tmdbService,
        IFileStorageService fileStorageService,
        IExportService exportService)
    {
        _context = context;
        _libraryService = libraryService;
        _tmdbService = tmdbService;
        _fileStorageService = fileStorageService;
        _exportService = exportService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchTerm,
        string? genre,
        SerieStatus? status,
        ContentType? contentType,
        bool favoritesOnly = false,
        string sortBy = "date_desc",
        int page = 1)
    {
        if (!TryGetUserContext(out var userId, out var isAdmin))
        {
            return Challenge();
        }

        var vm = await _libraryService.GetLibraryAsync(
            userId,
            isAdmin,
            new SeriesQueryOptions
            {
                SearchTerm = searchTerm,
                Genre = genre,
                Status = status,
                ContentType = contentType,
                FavoritesOnly = favoritesOnly,
                SortBy = sortBy,
                Page = page
            });

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        if (!TryGetUserContext(out var userId, out var isAdmin))
        {
            return Challenge();
        }

        var vm = await _libraryService.GetDashboardAsync(userId, isAdmin);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        if (!TryGetUserContext(out var userId, out var isAdmin))
        {
            return Challenge();
        }

        var item = await _libraryService.GetAccessibleByIdAsync(id.Value, userId, isAdmin);
        if (item is null)
        {
            var exists = await _context.Series.AsNoTracking().AnyAsync(s => s.Id == id.Value);
            TempData["ErrorMessage"] = exists
                ? "Ce contenu n'est pas accessible depuis votre compte."
                : "Ce contenu n'existe plus.";
            return RedirectToAction(nameof(Index));
        }

        TmdbMediaDetailsViewModel? tmdbDetails = null;
        if (_tmdbService.IsEnabled && item.TmdbId.HasValue)
        {
            var mediaType = item.ContentType == ContentType.Film ? "movie" : "tv";
            tmdbDetails = await _tmdbService.GetMediaDetailsAsync(mediaType, item.TmdbId.Value);
        }

        return View(new SeriesDetailsViewModel
        {
            Item = item,
            Tmdb = tmdbDetails
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Serie { DateAdded = DateTime.Today, Status = SerieStatus.Watchlist, ContentType = ContentType.Serie });
    }

    [HttpGet]
    public async Task<IActionResult> CreateFromTmdb(string mediaType, int tmdbId)
    {
        var imported = await _tmdbService.BuildItemFromTmdbAsync(mediaType, tmdbId);
        if (imported is null)
        {
            TempData["ErrorMessage"] = "Import TMDB impossible (verifie API key / id).";
            return RedirectToAction(nameof(Create));
        }

        TempData["SuccessMessage"] = "Contenu pre-rempli depuis TMDB. Verifie puis enregistre.";
        return View("Create", imported);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Title,Genre,Description,ContentType,SeasonsCount,TotalEpisodes,WatchedEpisodes,LastWatchedSeason,LastWatchedEpisode,Status,PersonalRating,StreamingPlatform,DateAdded,ReleaseDate,NextReleaseDate,AverageEpisodeRuntimeMinutes,PosterUrl,BackdropUrl,IsFavorite,PersonalComment,TmdbId")] Serie serie,
        IFormFile? posterFile)
    {
        if (!ModelState.IsValid)
        {
            return View(serie);
        }

        if (!TryGetUserContext(out var userId, out var isAdmin))
        {
            return Challenge();
        }

        var uploadedPath = await _fileStorageService.SavePosterAsync(posterFile);
        if (!string.IsNullOrWhiteSpace(uploadedPath))
        {
            serie.PosterUrl = uploadedPath;
        }

        serie.OwnerId = userId;
        if (serie.Status == SerieStatus.Watchlist)
        {
            serie.WatchlistOrder = await GetNextWatchlistOrderAsync(userId, isAdmin);
        }
        _context.Add(serie);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"'{serie.Title}' ajoute a votre bibliotheque.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        if (!TryGetUserContext(out var userId, out var isAdmin))
        {
            return Challenge();
        }

        var item = await _libraryService.GetAccessibleByIdAsync(id.Value, userId, isAdmin);
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Title,Genre,Description,ContentType,SeasonsCount,TotalEpisodes,WatchedEpisodes,LastWatchedSeason,LastWatchedEpisode,Status,PersonalRating,StreamingPlatform,DateAdded,ReleaseDate,NextReleaseDate,AverageEpisodeRuntimeMinutes,PosterUrl,BackdropUrl,IsFavorite,PersonalComment,TmdbId")] Serie input,
        IFormFile? posterFile)
    {
        if (id != input.Id)
        {
            return NotFound();
        }

        if (!TryGetUserContext(out var userId, out var isAdmin))
        {
            return Challenge();
        }

        var existing = await _libraryService.GetAccessibleByIdAsync(id, userId, isAdmin);
        if (existing is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(input);
        }

        ApplyEditableFields(existing, input);
        existing.WatchlistOrder = input.Status == SerieStatus.Watchlist
            ? existing.WatchlistOrder ?? await GetNextWatchlistOrderAsync(userId, isAdmin)
            : null;

        var uploadedPath = await _fileStorageService.SavePosterAsync(posterFile);
        if (!string.IsNullOrWhiteSpace(uploadedPath))
        {
            existing.PosterUrl = uploadedPath;
        }

        try
        {
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"'{existing.Title}' mis a jour avec succes.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Series.AnyAsync(s => s.Id == input.Id))
            {
                return NotFound();
            }

            ModelState.AddModelError(string.Empty, "Conflit de mise a jour detecte. Recharge la page.");
            return View(input);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> SearchSuggestions(string? query)
    {
        if (!TryGetUserContext(out var userId, out var isAdmin))
        {
            return Json(Array.Empty<object>());
        }

        var suggestions = await _libraryService.GetSearchSuggestionsAsync(userId, isAdmin, query ?? string.Empty, 8);
        return Json(suggestions);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReorderWatchlist([FromBody] WatchlistReorderRequest request)
    {
        if (!TryGetUserContext(out var userId, out var isAdmin))
        {
            return Unauthorized();
        }

        var ok = await _libraryService.ReorderWatchlistAsync(userId, isAdmin, request.OrderedIds);
        return ok ? Ok(new { message = "Ordre enregistre." }) : BadRequest(new { message = "Impossible de reordonner." });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        if (!TryGetUserContext(out var userId, out var isAdmin))
        {
            return Challenge();
        }

        var item = await _libraryService.GetAccessibleByIdAsync(id.Value, userId, isAdmin);
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!TryGetUserContext(out var userId, out var isAdmin))
        {
            return Challenge();
        }

        var item = await _libraryService.GetAccessibleByIdAsync(id, userId, isAdmin);
        if (item is null)
        {
            TempData["ErrorMessage"] = "Contenu introuvable.";
            return RedirectToAction(nameof(Index));
        }

        _context.Series.Remove(item);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"'{item.Title}' supprime.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsWatched(int id)
    {
        if (!TryGetUserContext(out var userId, out var isAdmin))
        {
            return Challenge();
        }

        var ok = await _libraryService.MarkAsWatchedAsync(id, userId, isAdmin);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Progression mise a jour."
            : "Impossible de marquer comme vu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
        if (!TryGetUserContext(out var userId, out var isAdmin))
        {
            return Challenge();
        }

        var ok = await _libraryService.ToggleFavoriteAsync(id, userId, isAdmin);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Favori mis a jour."
            : "Action impossible.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Calendar()
    {
        if (!TryGetUserContext(out var userId, out var isAdmin))
        {
            return Challenge();
        }

        var query = _context.Series.AsNoTracking().Where(s => s.NextReleaseDate.HasValue);
        if (!isAdmin)
        {
            query = query.Where(s => s.OwnerId == userId);
        }

        var upcoming = await query
            .OrderBy(s => s.NextReleaseDate)
            .ThenBy(s => s.Title)
            .Take(80)
            .ToListAsync();

        return View(upcoming);
    }

    [HttpGet]
    public async Task<IActionResult> TmdbSearch(string? query)
    {
        var vm = new TmdbSearchPageViewModel
        {
            Query = query ?? string.Empty,
            TmdbEnabled = _tmdbService.IsEnabled
        };

        if (string.IsNullOrWhiteSpace(query))
        {
            return View(vm);
        }

        if (!_tmdbService.IsEnabled)
        {
            vm.ErrorMessage = "TMDB desactive. Ajoute la cle API dans appsettings.json (Tmdb:ApiKey).";
            return View(vm);
        }

        vm.Results = await _tmdbService.SearchAsync(query);
        if (!vm.Results.Any())
        {
            vm.ErrorMessage = "Aucun resultat TMDB pour cette recherche.";
        }

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv()
    {
        if (!TryGetUserContext(out var userId, out var isAdmin))
        {
            return Challenge();
        }

        var bytes = await _exportService.BuildLibraryCsvAsync(userId, isAdmin);
        var fileName = $"bibliotheque_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    private bool TryGetUserContext(out string userId, out bool isAdmin)
    {
        userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        isAdmin = User.IsInRole(AppRoles.Admin);
        return !string.IsNullOrWhiteSpace(userId);
    }

    private static void ApplyEditableFields(Serie target, Serie source)
    {
        target.Title = source.Title;
        target.Genre = source.Genre;
        target.Description = source.Description;
        target.ContentType = source.ContentType;
        target.SeasonsCount = source.SeasonsCount;
        target.TotalEpisodes = source.TotalEpisodes;
        target.WatchedEpisodes = source.WatchedEpisodes;
        target.LastWatchedSeason = source.LastWatchedSeason;
        target.LastWatchedEpisode = source.LastWatchedEpisode;
        target.Status = source.Status;
        target.PersonalRating = source.PersonalRating;
        target.StreamingPlatform = source.StreamingPlatform;
        target.DateAdded = source.DateAdded;
        target.ReleaseDate = source.ReleaseDate;
        target.NextReleaseDate = source.NextReleaseDate;
        target.AverageEpisodeRuntimeMinutes = source.AverageEpisodeRuntimeMinutes;
        target.PosterUrl = source.PosterUrl;
        target.BackdropUrl = source.BackdropUrl;
        target.IsFavorite = source.IsFavorite;
        target.PersonalComment = source.PersonalComment;
        target.TmdbId = source.TmdbId;
    }

    private async Task<int> GetNextWatchlistOrderAsync(string userId, bool isAdmin)
    {
        var query = _context.Series.Where(s => s.Status == SerieStatus.Watchlist);
        if (!isAdmin)
        {
            query = query.Where(s => s.OwnerId == userId);
        }

        var max = await query.MaxAsync(s => (int?)s.WatchlistOrder) ?? 0;
        return max + 1;
    }
}
