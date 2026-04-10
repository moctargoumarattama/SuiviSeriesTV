using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SuiviSeriesTV.Configuration;
using SuiviSeriesTV.Constants;
using SuiviSeriesTV.Models;

namespace SuiviSeriesTV.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var settings = serviceProvider.GetRequiredService<IOptions<SeedAccountsSettings>>().Value;

        await EnsureRoleAsync(roleManager, AppRoles.Admin);
        await EnsureRoleAsync(roleManager, AppRoles.User);

        await EnsureUserAsync(
            userManager,
            settings.AdminEmail,
            settings.AdminPassword,
            AppRoles.Admin);

        var user = await EnsureUserAsync(
            userManager,
            settings.UserEmail,
            settings.UserPassword,
            AppRoles.User);

        var existingTitles = await context.Series
            .Where(s => s.OwnerId == user.Id)
            .Select(s => s.Title)
            .ToListAsync();

        var titleSet = new HashSet<string>(existingTitles, StringComparer.OrdinalIgnoreCase);
        var samples = BuildSampleCatalog(user.Id)
            .Where(s => !titleSet.Contains(s.Title))
            .ToList();

        if (samples.Count > 0)
        {
            context.Series.AddRange(samples);
            await context.SaveChangesAsync();
        }
    }

    private static List<Serie> BuildSampleCatalog(string ownerId)
    {
        var today = DateTime.UtcNow.Date;

        return
        [
            CreateSeries(ownerId, today, "Breaking Bad", "Drame", "Un professeur de chimie se lance dans la methamphetamine.", SerieStatus.Termine, 9.8, "Netflix", 5, 62, 62, 5, 62, 30, posterUrl: "https://image.tmdb.org/t/p/w500/ztkUQFLlC19CCMYHW9o1zWhJRNq.jpg", backdropUrl: "https://image.tmdb.org/t/p/original/9faGSFi5jam6pDWGNd0p8JcJgXQ.jpg"),
            CreateSeries(ownerId, today, "Better Call Saul", "Drame", "L'ascension de Saul Goodman avant Breaking Bad.", SerieStatus.Termine, 9.1, "Netflix", 6, 63, 63, 6, 63, 28),
            CreateSeries(ownerId, today, "The Last of Us", "Science-fiction", "Une histoire de survie dans un monde post-apocalyptique.", SerieStatus.EnCours, 8.7, "Max", 1, 9, 5, 1, 5, 12, nextReleaseDate: today.AddDays(3), posterUrl: "https://image.tmdb.org/t/p/w500/uKvVjHNqB5VmOrdxqAt2F7J78ED.jpg", backdropUrl: "https://image.tmdb.org/t/p/original/uDgy6hyPd82kOHh6I95FLtLnj6p.jpg"),
            CreateSeries(ownerId, today, "Stranger Things", "Fantastique", "Une bande d'amis affronte des menaces venues d'une autre dimension.", SerieStatus.EnCours, 9.0, "Netflix", 4, 34, 26, 4, 9, 21, true),
            CreateSeries(ownerId, today, "Dark", "Mystere", "Des disparitions troublantes revelent un secret temporel.", SerieStatus.Termine, 9.0, "Netflix", 3, 26, 26, 3, 26, 19),
            CreateSeries(ownerId, today, "Peaky Blinders", "Crime", "Le gang Shelby impose sa loi dans Birmingham.", SerieStatus.EnCours, 8.8, "Netflix", 6, 36, 18, 4, 6, 17),
            CreateSeries(ownerId, today, "The Bear", "Comedie, Drame", "Un chef tente de sauver le restaurant familial.", SerieStatus.EnCours, 8.5, "Disney+", 3, 28, 14, 2, 4, 14),
            CreateSeries(ownerId, today, "House of the Dragon", "Aventure, Drame", "La lutte de succession des Targaryen divise le royaume.", SerieStatus.Watchlist, 8.6, "Max", 2, 18, 0, 0, 0, 13, false, 8, nextReleaseDate: today.AddDays(24)),
            CreateSeries(ownerId, today, "Wednesday", "Fantastique", "Wednesday Addams mene l'enquete dans son ecole.", SerieStatus.Termine, 8.1, "Netflix", 1, 8, 8, 1, 8, 11),
            CreateSeries(ownerId, today, "The Mandalorian", "Action, Aventure", "Un chasseur de primes parcourt la galaxie avec Grogu.", SerieStatus.EnCours, 8.6, "Disney+", 3, 24, 16, 2, 8, 9),
            CreateSeries(ownerId, today, "Chernobyl", "Historique, Drame", "Le recit glaçant de la catastrophe nucleaire.", SerieStatus.Termine, 9.4, "Max", 1, 5, 5, 1, 5, 8),
            CreateSeries(ownerId, today, "Severance", "Science-fiction", "Une entreprise separe memories pro et perso de ses employes.", SerieStatus.Watchlist, 8.8, "Apple TV+", 2, 19, 0, 0, 0, 6, false, 9, nextReleaseDate: today.AddDays(14)),
            CreateSeries(ownerId, today, "Black Mirror", "Science-fiction", "Chaque episode explore les derives de la technologie.", SerieStatus.Watchlist, 8.5, "Netflix", 6, 27, 0, 0, 0, 5, false, 10),
            CreateSeries(ownerId, today, "One Piece", "Anime, Aventure", "Luffy et son equipage partent a la recherche du One Piece.", SerieStatus.EnCours, 8.9, "Crunchyroll", 20, 1100, 120, 6, 120, 4, runtime: 24, contentType: ContentType.Anime),
            CreateSeries(ownerId, today, "Attack on Titan", "Anime, Action", "L'humanite lutte pour survivre face aux titans.", SerieStatus.Termine, 9.2, "Crunchyroll", 4, 89, 89, 4, 89, 16, runtime: 24, contentType: ContentType.Anime),
            CreateSeries(ownerId, today, "Arcane", "Anime, Action", "Deux soeurs se retrouvent opposees entre Piltover et Zaun.", SerieStatus.Termine, 9.3, "Netflix", 1, 9, 9, 1, 9, 10, runtime: 40, contentType: ContentType.Anime),
            CreateSeries(ownerId, today, "Frieren", "Anime, Fantastique", "Une mage immortelle voyage pour comprendre les humains.", SerieStatus.EnCours, 8.9, "Crunchyroll", 1, 28, 12, 1, 12, 3, runtime: 24, contentType: ContentType.Anime),
            CreateSeries(ownerId, today, "Naruto Shippuden", "Anime, Action", "Naruto poursuit sa quete pour proteger son village.", SerieStatus.EnCours, 8.7, "Crunchyroll", 21, 500, 210, 9, 210, 2, runtime: 24, contentType: ContentType.Anime),

            CreateFilm(ownerId, today, "Dune: Part Two", "Science-fiction", "Paul Atreides s'unit aux Fremen pour prendre sa revanche.", SerieStatus.Watchlist, 8.8, "Cinema / VOD", 0, 10, 1, runtime: 166, releaseDate: today.AddDays(-25), nextReleaseDate: today.AddDays(7), posterUrl: "https://image.tmdb.org/t/p/w500/1pdfLvkbY9ohJlCjQH2CZjjYVvJ.jpg", backdropUrl: "https://image.tmdb.org/t/p/original/sR0SpCrXamlIkYMdfz83sFn5JS6.jpg"),
            CreateFilm(ownerId, today, "Inception", "Action", "Un voleur infiltre les reves pour implanter une idee.", SerieStatus.Watchlist, 9.0, "Netflix", 0, 9, 2, runtime: 148, releaseDate: new DateTime(2010, 7, 16), posterUrl: "https://image.tmdb.org/t/p/w500/oYuLEt3zVCKq57qu2F8dT7NIa6f.jpg", backdropUrl: "https://image.tmdb.org/t/p/original/s3TBrRGB1iav7gFOCNx3H31MoES.jpg"),
            CreateFilm(ownerId, today, "Spider-Man: Across the Spider-Verse", "Animation", "Miles Morales voyage entre les dimensions du Spider-Verse.", SerieStatus.Watchlist, 8.7, "Netflix", 0, 8, 3, runtime: 140, releaseDate: new DateTime(2023, 6, 2), posterUrl: "https://image.tmdb.org/t/p/w500/8Vt6mWEReuy4Of61Lnj5Xj704m8.jpg", backdropUrl: "https://image.tmdb.org/t/p/original/4HodYYKEIsGOdinkGi2Ucz6X9i0.jpg"),
            CreateFilm(ownerId, today, "Your Name", "Anime", "Deux adolescents echanges a distance cherchent a se retrouver.", SerieStatus.Watchlist, 8.6, "Crunchyroll", 0, 7, 4, runtime: 112, releaseDate: new DateTime(2016, 8, 26), posterUrl: "https://image.tmdb.org/t/p/w500/q719jXXEzOoYaps6babgKnONONX.jpg", backdropUrl: "https://image.tmdb.org/t/p/original/mMtUybQ6hL24FXo0F3Z4j2KG7kZ.jpg"),
            CreateFilm(ownerId, today, "Avatar: The Way of Water", "Aventure", "Jake Sully doit proteger sa famille dans Pandora.", SerieStatus.Watchlist, 8.0, "Disney+", 0, 6, 5, runtime: 192, releaseDate: new DateTime(2022, 12, 14)),
            CreateFilm(ownerId, today, "The Batman", "Thriller", "Un Batman plus sombre traque le Riddler a Gotham.", SerieStatus.Watchlist, 8.1, "Max", 0, 5, 6, runtime: 176, releaseDate: new DateTime(2022, 3, 2)),
            CreateFilm(ownerId, today, "The Matrix", "Science-fiction", "Neo decouvre la verite sur la Matrice.", SerieStatus.Watchlist, 8.7, "Prime Video", 0, 5, 7, runtime: 136, releaseDate: new DateTime(1999, 3, 31)),
            CreateFilm(ownerId, today, "John Wick: Chapter 4", "Action", "John Wick affronte la Table dans un combat mondial.", SerieStatus.Watchlist, 8.2, "Prime Video", 0, 4, 11, runtime: 169, releaseDate: new DateTime(2023, 3, 22)),
            CreateFilm(ownerId, today, "The Lord of the Rings: The Fellowship of the Ring", "Fantastique", "La Communaute se forme pour detruire l'Anneau unique.", SerieStatus.Watchlist, 8.9, "Prime Video", 0, 3, 12, runtime: 178, releaseDate: new DateTime(2001, 12, 19)),

            CreateFilm(ownerId, today, "Interstellar", "Science-fiction", "Une equipe traverse un trou de ver pour sauver l'humanite.", SerieStatus.Termine, 9.3, "Prime Video", 1, 8, null, true, 169, new DateTime(2014, 11, 7), posterUrl: "https://image.tmdb.org/t/p/w500/gEU2QniE6E77NI6lCU6MxlNBvIx.jpg", backdropUrl: "https://image.tmdb.org/t/p/original/rAiYTfKGqDCRIIqo664sY9XZIvQ.jpg"),
            CreateFilm(ownerId, today, "The Dark Knight", "Thriller", "Batman affronte le Joker a Gotham.", SerieStatus.Termine, 9.4, "Max", 1, 6, null, true, 152, new DateTime(2008, 7, 18), posterUrl: "https://image.tmdb.org/t/p/w500/qJ2tW6WMUDux911r6m7haRef0WH.jpg", backdropUrl: "https://image.tmdb.org/t/p/original/nMKdUUepR0i5zn0y1T4CsSB5chy.jpg"),
            CreateFilm(ownerId, today, "Parasite", "Drame", "Une famille modeste infiltre le quotidien d'une famille riche.", SerieStatus.Termine, 8.9, "Canal+", 1, 5, runtime: 132, releaseDate: new DateTime(2019, 5, 30), posterUrl: "https://image.tmdb.org/t/p/w500/7IiTTgloJzvGI1TAYymCfbfl3vT.jpg", backdropUrl: "https://image.tmdb.org/t/p/original/TU9NIjwzjoKPwQHoHshkFcQUCG.jpg"),
            CreateFilm(ownerId, today, "Oppenheimer", "Historique", "Le parcours du pere de la bombe atomique.", SerieStatus.Termine, 8.8, "VOD", 1, 4, runtime: 181, releaseDate: new DateTime(2023, 7, 19)),
            CreateFilm(ownerId, today, "Blade Runner 2049", "Science-fiction", "Un blade runner decouvre un secret capable de tout bouleverser.", SerieStatus.Termine, 8.2, "Netflix", 1, 3, runtime: 164, releaseDate: new DateTime(2017, 10, 4)),
            CreateFilm(ownerId, today, "Whiplash", "Drame", "Un batteur ambitieux subit la pression d'un professeur extremiste.", SerieStatus.Termine, 8.5, "Prime Video", 1, 3, runtime: 107, releaseDate: new DateTime(2014, 10, 10)),
            CreateFilm(ownerId, today, "Mad Max: Fury Road", "Action", "Une fuite explosive dans un desert post-apocalyptique.", SerieStatus.Termine, 8.1, "Max", 1, 3, runtime: 120, releaseDate: new DateTime(2015, 5, 14)),
            CreateFilm(ownerId, today, "Joker", "Thriller", "La descente d'Arthur Fleck vers le chaos.", SerieStatus.Termine, 8.4, "Max", 1, 2, runtime: 122, releaseDate: new DateTime(2019, 10, 4)),
            CreateFilm(ownerId, today, "Pulp Fiction", "Crime", "Destins croises dans un Los Angeles violent et ironique.", SerieStatus.Termine, 9.0, "Netflix", 1, 2, runtime: 154, releaseDate: new DateTime(1994, 10, 14)),
            CreateFilm(ownerId, today, "Fight Club", "Drame", "Un homme cree un club clandestin qui degenere.", SerieStatus.Termine, 8.8, "Prime Video", 1, 2, runtime: 139, releaseDate: new DateTime(1999, 10, 15)),
            CreateFilm(ownerId, today, "Spirited Away", "Anime", "Chihiro entre dans un monde magique pour sauver ses parents.", SerieStatus.Termine, 8.9, "Netflix", 1, 2, runtime: 125, releaseDate: new DateTime(2001, 7, 20)),
            CreateFilm(ownerId, today, "Coco", "Animation", "Miguel part a la rencontre de ses ancetres dans l'au-dela.", SerieStatus.Termine, 8.3, "Disney+", 1, 2, runtime: 105, releaseDate: new DateTime(2017, 11, 22)),
            CreateFilm(ownerId, today, "Everything Everywhere All at Once", "Science-fiction", "Une femme ordinaire plonge dans le multivers.", SerieStatus.Termine, 8.4, "Prime Video", 1, 1, runtime: 139, releaseDate: new DateTime(2022, 3, 25)),
            CreateFilm(ownerId, today, "Gladiator", "Action", "Un general romain trahi revient comme gladiateur.", SerieStatus.Termine, 8.5, "Netflix", 1, 1, runtime: 155, releaseDate: new DateTime(2000, 5, 5)),
            CreateFilm(ownerId, today, "Arrival", "Science-fiction", "Une linguiste tente de communiquer avec des extraterrestres.", SerieStatus.Termine, 8.1, "Prime Video", 1, 1, runtime: 116, releaseDate: new DateTime(2016, 11, 11)),
            CreateFilm(ownerId, today, "Dune", "Science-fiction", "Paul Atreides decouvre son destin sur Arrakis.", SerieStatus.Termine, 8.2, "Max", 1, 1, runtime: 155, releaseDate: new DateTime(2021, 9, 15)),
            CreateFilm(ownerId, today, "Avengers: Endgame", "Action", "Les Avengers tentent de restaurer l'univers.", SerieStatus.Termine, 8.4, "Disney+", 1, 1, runtime: 181, releaseDate: new DateTime(2019, 4, 26)),
            CreateFilm(ownerId, today, "La La Land", "Comedie musicale", "Deux artistes poursuivent leurs reves a Los Angeles.", SerieStatus.Termine, 8.0, "Canal+", 1, 1, runtime: 128, releaseDate: new DateTime(2016, 12, 9))
        ];
    }

    private static Serie CreateSeries(
        string ownerId,
        DateTime today,
        string title,
        string genre,
        string description,
        SerieStatus status,
        double rating,
        string platform,
        int seasons,
        int totalEpisodes,
        int watchedEpisodes,
        int lastSeason,
        int lastEpisode,
        int addedDaysAgo,
        bool favorite = false,
        int? watchlistOrder = null,
        DateTime? nextReleaseDate = null,
        int runtime = 45,
        ContentType contentType = ContentType.Serie,
        DateTime? releaseDate = null,
        string? posterUrl = null,
        string? backdropUrl = null)
    {
        var safeSeasons = Math.Max(1, seasons);
        var safeTotal = Math.Max(1, totalEpisodes);
        var safeWatched = Math.Clamp(watchedEpisodes, 0, safeTotal);
        var safeLastEpisode = Math.Clamp(lastEpisode, 0, safeWatched);
        var safeLastSeason = Math.Clamp(lastSeason, 0, safeSeasons);

        return new Serie
        {
            Title = title,
            Genre = genre,
            Description = description,
            SeasonsCount = safeSeasons,
            TotalEpisodes = safeTotal,
            WatchedEpisodes = safeWatched,
            Status = status,
            PersonalRating = rating,
            StreamingPlatform = platform,
            DateAdded = today.AddDays(-Math.Abs(addedDaysAgo)),
            ContentType = contentType,
            LastWatchedSeason = safeLastSeason,
            LastWatchedEpisode = safeLastEpisode,
            NextReleaseDate = nextReleaseDate,
            ReleaseDate = releaseDate,
            AverageEpisodeRuntimeMinutes = runtime,
            IsFavorite = favorite,
            WatchlistOrder = status == SerieStatus.Watchlist ? watchlistOrder : null,
            PosterUrl = posterUrl ?? BuildSeedImage(title, "poster", 500, 750),
            BackdropUrl = backdropUrl ?? BuildSeedImage(title, "backdrop", 1280, 720),
            OwnerId = ownerId
        };
    }

    private static Serie CreateFilm(
        string ownerId,
        DateTime today,
        string title,
        string genre,
        string description,
        SerieStatus status,
        double rating,
        string platform,
        int watchedEpisodes,
        int addedDaysAgo,
        int? watchlistOrder = null,
        bool favorite = false,
        int runtime = 120,
        DateTime? releaseDate = null,
        DateTime? nextReleaseDate = null,
        string? posterUrl = null,
        string? backdropUrl = null)
    {
        var safeWatched = Math.Clamp(watchedEpisodes, 0, 1);

        return new Serie
        {
            Title = title,
            Genre = genre,
            Description = description,
            SeasonsCount = 1,
            TotalEpisodes = 1,
            WatchedEpisodes = safeWatched,
            Status = status,
            PersonalRating = rating,
            StreamingPlatform = platform,
            DateAdded = today.AddDays(-Math.Abs(addedDaysAgo)),
            ContentType = ContentType.Film,
            LastWatchedSeason = safeWatched > 0 ? 1 : 0,
            LastWatchedEpisode = safeWatched > 0 ? 1 : 0,
            NextReleaseDate = nextReleaseDate,
            ReleaseDate = releaseDate,
            AverageEpisodeRuntimeMinutes = runtime,
            IsFavorite = favorite,
            WatchlistOrder = status == SerieStatus.Watchlist ? watchlistOrder : null,
            PosterUrl = posterUrl ?? BuildSeedImage(title, "poster", 500, 750),
            BackdropUrl = backdropUrl ?? BuildSeedImage(title, "backdrop", 1280, 720),
            OwnerId = ownerId
        };
    }

    private static string BuildSeedImage(string title, string kind, int width, int height)
    {
        var normalized = Uri.EscapeDataString($"{title}-{kind}".ToLowerInvariant().Replace(' ', '-'));
        return $"https://picsum.photos/seed/{normalized}/{width}/{height}";
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create seed user '{email}': {errors}");
            }
        }
        else if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }

        return user;
    }
}

