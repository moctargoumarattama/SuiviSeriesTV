using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SuiviSeriesTV.Configuration;
using SuiviSeriesTV.Models;
using SuiviSeriesTV.ViewModels.Tmdb;

namespace SuiviSeriesTV.Services.Tmdb;

public class TmdbService : ITmdbService
{
    private readonly HttpClient _httpClient;
    private readonly TmdbSettings _settings;

    public TmdbService(HttpClient httpClient, IOptions<TmdbSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public bool IsEnabled => _settings.Enabled && !string.IsNullOrWhiteSpace(_settings.ApiKey);

    public async Task<IReadOnlyList<TmdbSearchResultViewModel>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled || string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var url = BuildUrl($"/search/multi?query={Uri.EscapeDataString(query)}&language=fr-FR&include_adult=false");
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("results", out var resultsNode) || resultsNode.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var list = new List<TmdbSearchResultViewModel>();
        foreach (var item in resultsNode.EnumerateArray())
        {
            var mediaType = item.TryGetProperty("media_type", out var mediaNode)
                ? mediaNode.GetString()
                : null;

            if (mediaType is not ("movie" or "tv"))
            {
                continue;
            }

            var title = mediaType == "movie"
                ? item.GetPropertyOrDefault("title")
                : item.GetPropertyOrDefault("name");

            list.Add(new TmdbSearchResultViewModel
            {
                TmdbId = item.GetPropertyOrDefaultInt("id"),
                MediaType = mediaType,
                Title = title ?? "Sans titre",
                Overview = item.GetPropertyOrDefault("overview") ?? string.Empty,
                PosterUrl = BuildImageUrl(item.GetPropertyOrDefault("poster_path")),
                BackdropUrl = BuildImageUrl(item.GetPropertyOrDefault("backdrop_path")),
                ReleaseDate = ParseDate(mediaType == "movie"
                    ? item.GetPropertyOrDefault("release_date")
                    : item.GetPropertyOrDefault("first_air_date")),
                VoteAverage = item.GetPropertyOrDefaultDouble("vote_average")
            });
        }

        return list
            .OrderByDescending(x => x.VoteAverage ?? 0)
            .ThenBy(x => x.Title)
            .Take(30)
            .ToList();
    }

    public async Task<Serie?> BuildItemFromTmdbAsync(string mediaType, int tmdbId, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled || tmdbId <= 0 || string.IsNullOrWhiteSpace(mediaType))
        {
            return null;
        }

        var endpoint = mediaType == "movie" ? $"/movie/{tmdbId}" : $"/tv/{tmdbId}";
        var url = BuildUrl($"{endpoint}?language=fr-FR");

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;

        var title = mediaType == "movie"
            ? root.GetPropertyOrDefault("title")
            : root.GetPropertyOrDefault("name");

        var genreNames = root.TryGetProperty("genres", out var genresNode) && genresNode.ValueKind == JsonValueKind.Array
            ? string.Join(", ", genresNode.EnumerateArray().Select(g => g.GetPropertyOrDefault("name")).Where(x => !string.IsNullOrWhiteSpace(x)))
            : "Inconnu";

        var contentType = ResolveContentType(mediaType, genreNames);
        var releaseDateRaw = mediaType == "movie"
            ? root.GetPropertyOrDefault("release_date")
            : root.GetPropertyOrDefault("first_air_date");

        var totalEpisodes = mediaType == "movie"
            ? 1
            : Math.Max(1, root.GetPropertyOrDefaultInt("number_of_episodes"));

        var seasons = mediaType == "movie"
            ? 1
            : Math.Max(1, root.GetPropertyOrDefaultInt("number_of_seasons"));

        var runtime = mediaType == "movie"
            ? root.GetPropertyOrDefaultInt("runtime")
            : root.GetArrayFirstInt("episode_run_time");

        return new Serie
        {
            Title = title ?? "Sans titre",
            Description = root.GetPropertyOrDefault("overview") ?? "Aucune description.",
            Genre = string.IsNullOrWhiteSpace(genreNames) ? "Inconnu" : genreNames,
            ContentType = contentType,
            SeasonsCount = seasons,
            TotalEpisodes = totalEpisodes,
            WatchedEpisodes = 0,
            LastWatchedSeason = 0,
            LastWatchedEpisode = 0,
            Status = SerieStatus.Watchlist,
            PersonalRating = Math.Round(root.GetPropertyOrDefaultDouble("vote_average") ?? 0, 1),
            StreamingPlatform = "Non specifie",
            DateAdded = DateTime.UtcNow.Date,
            ReleaseDate = ParseDate(releaseDateRaw),
            PosterUrl = BuildImageUrl(root.GetPropertyOrDefault("poster_path")),
            BackdropUrl = BuildImageUrl(root.GetPropertyOrDefault("backdrop_path")),
            AverageEpisodeRuntimeMinutes = runtime > 0 ? runtime : null,
            TmdbId = tmdbId
        };
    }

    public async Task<TmdbMediaDetailsViewModel?> GetMediaDetailsAsync(string mediaType, int tmdbId, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled || tmdbId <= 0 || string.IsNullOrWhiteSpace(mediaType))
        {
            return null;
        }

        var endpoint = mediaType == "movie" ? $"/movie/{tmdbId}" : $"/tv/{tmdbId}";
        var url = BuildUrl($"{endpoint}?language=fr-FR&append_to_response=videos,credits");

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;

        var genres = root.TryGetProperty("genres", out var genresNode) && genresNode.ValueKind == JsonValueKind.Array
            ? genresNode.EnumerateArray()
                .Select(g => g.GetPropertyOrDefault("name"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Take(5)
                .Cast<string>()
                .ToList()
            : [];

        var trailer = root.TryGetProperty("videos", out var videosNode)
            ? ExtractTrailer(videosNode)
            : null;

        var cast = root.TryGetProperty("credits", out var creditsNode) && creditsNode.TryGetProperty("cast", out var castNode) && castNode.ValueKind == JsonValueKind.Array
            ? castNode.EnumerateArray()
                .Select(c => c.GetPropertyOrDefault("name"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Take(6)
                .Cast<string>()
                .ToList()
            : [];

        var runtime = mediaType == "movie"
            ? root.GetPropertyOrDefaultInt("runtime")
            : root.GetArrayFirstInt("episode_run_time");

        return new TmdbMediaDetailsViewModel
        {
            TrailerUrl = trailer?.Url,
            TrailerName = trailer?.Name,
            Cast = cast,
            Genres = genres,
            RuntimeMinutes = runtime > 0 ? runtime : null
        };
    }

    private string BuildUrl(string relative)
    {
        var separator = relative.Contains('?') ? '&' : '?';
        return $"{_settings.BaseUrl.TrimEnd('/')}{relative}{separator}api_key={_settings.ApiKey}";
    }

    private string? BuildImageUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return $"{_settings.ImageBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
            ? dt
            : null;
    }

    private static ContentType ResolveContentType(string mediaType, string genreNames)
    {
        if (mediaType == "movie")
        {
            return ContentType.Film;
        }

        var lower = genreNames.ToLowerInvariant();
        if (lower.Contains("animation") || lower.Contains("anime"))
        {
            return ContentType.Anime;
        }

        return ContentType.Serie;
    }

    private static TrailerInfo? ExtractTrailer(JsonElement videosNode)
    {
        if (!videosNode.TryGetProperty("results", out var resultsNode) || resultsNode.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var preferred = resultsNode.EnumerateArray()
            .FirstOrDefault(v =>
                string.Equals(v.GetPropertyOrDefault("site"), "YouTube", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(v.GetPropertyOrDefault("type"), "Trailer", StringComparison.OrdinalIgnoreCase));

        if (preferred.ValueKind == JsonValueKind.Undefined)
        {
            preferred = resultsNode.EnumerateArray()
                .FirstOrDefault(v => string.Equals(v.GetPropertyOrDefault("site"), "YouTube", StringComparison.OrdinalIgnoreCase));
        }

        if (preferred.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        var key = preferred.GetPropertyOrDefault("key");
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return new TrailerInfo(
            $"https://www.youtube.com/watch?v={key}",
            preferred.GetPropertyOrDefault("name"));
    }
}

internal sealed record TrailerInfo(string Url, string? Name);

internal static class JsonExtensions
{
    public static string? GetPropertyOrDefault(this JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind != JsonValueKind.Null
            ? prop.GetString()
            : null;
    }

    public static int GetPropertyOrDefaultInt(this JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.TryGetInt32(out var value)
            ? value
            : 0;
    }

    public static double? GetPropertyOrDefaultDouble(this JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.TryGetDouble(out var value)
            ? value
            : null;
    }

    public static int GetArrayFirstInt(this JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var prop) || prop.ValueKind != JsonValueKind.Array)
        {
            return 0;
        }

        foreach (var item in prop.EnumerateArray())
        {
            if (item.TryGetInt32(out var value))
            {
                return value;
            }
        }

        return 0;
    }
}
