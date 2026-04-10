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
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var vm = await _libraryService.GetLibraryAsync(
            userId,
            IsAdmin(),
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
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var vm = await _libraryService.GetDashboardAsync(userId, IsAdmin());
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var item = await _libraryService.GetAccessibleByIdAsync(id.Value, userId, IsAdmin());
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
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

        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var uploadedPath = await _fileStorageService.SavePosterAsync(posterFile);
        if (!string.IsNullOrWhiteSpace(uploadedPath))
        {
            serie.PosterUrl = uploadedPath;
        }

        serie.OwnerId = userId;
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

        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var item = await _libraryService.GetAccessibleByIdAsync(id.Value, userId, IsAdmin());
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

        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var existing = await _libraryService.GetAccessibleByIdAsync(id, userId, IsAdmin());
        if (existing is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(input);
        }

        existing.Title = input.Title;
        existing.Genre = input.Genre;
        existing.Description = input.Description;
        existing.ContentType = input.ContentType;
        existing.SeasonsCount = input.SeasonsCount;
        existing.TotalEpisodes = input.TotalEpisodes;
        existing.WatchedEpisodes = input.WatchedEpisodes;
        existing.LastWatchedSeason = input.LastWatchedSeason;
        existing.LastWatchedEpisode = input.LastWatchedEpisode;
        existing.Status = input.Status;
        existing.PersonalRating = input.PersonalRating;
        existing.StreamingPlatform = input.StreamingPlatform;
        existing.DateAdded = input.DateAdded;
        existing.ReleaseDate = input.ReleaseDate;
        existing.NextReleaseDate = input.NextReleaseDate;
        existing.AverageEpisodeRuntimeMinutes = input.AverageEpisodeRuntimeMinutes;
        existing.PosterUrl = input.PosterUrl;
        existing.BackdropUrl = input.BackdropUrl;
        existing.IsFavorite = input.IsFavorite;
        existing.PersonalComment = input.PersonalComment;
        existing.TmdbId = input.TmdbId;

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
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var item = await _libraryService.GetAccessibleByIdAsync(id.Value, userId, IsAdmin());
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
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var item = await _libraryService.GetAccessibleByIdAsync(id, userId, IsAdmin());
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
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var ok = await _libraryService.MarkAsWatchedAsync(id, userId, IsAdmin());
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Progression mise a jour."
            : "Impossible de marquer comme vu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var ok = await _libraryService.ToggleFavoriteAsync(id, userId, IsAdmin());
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Favori mis a jour."
            : "Action impossible.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Calendar()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var query = _context.Series.AsNoTracking().Where(s => s.NextReleaseDate.HasValue);
        if (!IsAdmin())
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
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var bytes = await _exportService.BuildLibraryCsvAsync(userId, IsAdmin());
        var fileName = $"bibliotheque_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    private bool IsAdmin()
    {
        return User.IsInRole(AppRoles.Admin);
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
