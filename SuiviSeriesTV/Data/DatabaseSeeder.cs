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
            new Serie
            {
                Title = "Breaking Bad",
                Genre = "Drame",
                Description = "Un professeur de chimie se lance dans la methamphetamine.",
                SeasonsCount = 5,
                TotalEpisodes = 62,
                WatchedEpisodes = 62,
                Status = SerieStatus.Termine,
                PersonalRating = 9.8,
                StreamingPlatform = "Netflix",
                DateAdded = today.AddDays(-30),
                PosterUrl = "https://image.tmdb.org/t/p/w500/ztkUQFLlC19CCMYHW9o1zWhJRNq.jpg",
                BackdropUrl = "https://image.tmdb.org/t/p/original/9faGSFi5jam6pDWGNd0p8JcJgXQ.jpg",
                ContentType = ContentType.Serie,
                LastWatchedSeason = 5,
                LastWatchedEpisode = 62,
                OwnerId = ownerId
            },
            new Serie
            {
                Title = "The Last of Us",
                Genre = "Science-fiction",
                Description = "Une histoire de survie dans un monde post-apocalyptique.",
                SeasonsCount = 1,
                TotalEpisodes = 9,
                WatchedEpisodes = 5,
                Status = SerieStatus.EnCours,
                PersonalRating = 8.7,
                StreamingPlatform = "Max",
                DateAdded = today.AddDays(-12),
                PosterUrl = "https://image.tmdb.org/t/p/w500/uKvVjHNqB5VmOrdxqAt2F7J78ED.jpg",
                BackdropUrl = "https://image.tmdb.org/t/p/original/uDgy6hyPd82kOHh6I95FLtLnj6p.jpg",
                ContentType = ContentType.Serie,
                LastWatchedSeason = 1,
                LastWatchedEpisode = 5,
                NextReleaseDate = today.AddDays(3),
                OwnerId = ownerId
            },
            new Serie
            {
                Title = "Dune: Part Two",
                Genre = "Science-fiction",
                Description = "Paul Atreides s'unit aux Fremen pour prendre sa revanche.",
                SeasonsCount = 1,
                TotalEpisodes = 1,
                WatchedEpisodes = 0,
                Status = SerieStatus.Watchlist,
                PersonalRating = 8.8,
                StreamingPlatform = "Cinema / VOD",
                DateAdded = today.AddDays(-10),
                ContentType = ContentType.Film,
                PosterUrl = "https://image.tmdb.org/t/p/w500/1pdfLvkbY9ohJlCjQH2CZjjYVvJ.jpg",
                BackdropUrl = "https://image.tmdb.org/t/p/original/sR0SpCrXamlIkYMdfz83sFn5JS6.jpg",
                ReleaseDate = today.AddDays(-25),
                NextReleaseDate = today.AddDays(7),
                AverageEpisodeRuntimeMinutes = 166,
                LastWatchedSeason = 0,
                LastWatchedEpisode = 0,
                OwnerId = ownerId
            },
            new Serie
            {
                Title = "Interstellar",
                Genre = "Science-fiction",
                Description = "Une equipe traverse un trou de ver pour sauver l'humanite.",
                SeasonsCount = 1,
                TotalEpisodes = 1,
                WatchedEpisodes = 1,
                Status = SerieStatus.Termine,
                PersonalRating = 9.3,
                StreamingPlatform = "Prime Video",
                DateAdded = today.AddDays(-8),
                ContentType = ContentType.Film,
                PosterUrl = "https://image.tmdb.org/t/p/w500/gEU2QniE6E77NI6lCU6MxlNBvIx.jpg",
                BackdropUrl = "https://image.tmdb.org/t/p/original/rAiYTfKGqDCRIIqo664sY9XZIvQ.jpg",
                ReleaseDate = new DateTime(2014, 11, 7),
                AverageEpisodeRuntimeMinutes = 169,
                LastWatchedSeason = 1,
                LastWatchedEpisode = 1,
                IsFavorite = true,
                OwnerId = ownerId
            },
            new Serie
            {
                Title = "Inception",
                Genre = "Action",
                Description = "Un voleur infiltre les reves pour implanter une idee.",
                SeasonsCount = 1,
                TotalEpisodes = 1,
                WatchedEpisodes = 0,
                Status = SerieStatus.Watchlist,
                PersonalRating = 9.0,
                StreamingPlatform = "Netflix",
                DateAdded = today.AddDays(-7),
                ContentType = ContentType.Film,
                PosterUrl = "https://image.tmdb.org/t/p/w500/oYuLEt3zVCKq57qu2F8dT7NIa6f.jpg",
                BackdropUrl = "https://image.tmdb.org/t/p/original/s3TBrRGB1iav7gFOCNx3H31MoES.jpg",
                ReleaseDate = new DateTime(2010, 7, 16),
                AverageEpisodeRuntimeMinutes = 148,
                LastWatchedSeason = 0,
                LastWatchedEpisode = 0,
                OwnerId = ownerId
            },
            new Serie
            {
                Title = "The Dark Knight",
                Genre = "Thriller",
                Description = "Batman affronte le Joker a Gotham.",
                SeasonsCount = 1,
                TotalEpisodes = 1,
                WatchedEpisodes = 1,
                Status = SerieStatus.Termine,
                PersonalRating = 9.4,
                StreamingPlatform = "Max",
                DateAdded = today.AddDays(-6),
                ContentType = ContentType.Film,
                PosterUrl = "https://image.tmdb.org/t/p/w500/qJ2tW6WMUDux911r6m7haRef0WH.jpg",
                BackdropUrl = "https://image.tmdb.org/t/p/original/nMKdUUepR0i5zn0y1T4CsSB5chy.jpg",
                ReleaseDate = new DateTime(2008, 7, 18),
                AverageEpisodeRuntimeMinutes = 152,
                LastWatchedSeason = 1,
                LastWatchedEpisode = 1,
                IsFavorite = true,
                OwnerId = ownerId
            },
            new Serie
            {
                Title = "Parasite",
                Genre = "Drame",
                Description = "Une famille modeste infiltre le quotidien d'une famille riche.",
                SeasonsCount = 1,
                TotalEpisodes = 1,
                WatchedEpisodes = 1,
                Status = SerieStatus.Termine,
                PersonalRating = 8.9,
                StreamingPlatform = "Canal+",
                DateAdded = today.AddDays(-5),
                ContentType = ContentType.Film,
                PosterUrl = "https://image.tmdb.org/t/p/w500/7IiTTgloJzvGI1TAYymCfbfl3vT.jpg",
                BackdropUrl = "https://image.tmdb.org/t/p/original/TU9NIjwzjoKPwQHoHshkFcQUCG.jpg",
                ReleaseDate = new DateTime(2019, 5, 30),
                AverageEpisodeRuntimeMinutes = 132,
                LastWatchedSeason = 1,
                LastWatchedEpisode = 1,
                OwnerId = ownerId
            },
            new Serie
            {
                Title = "Spider-Man: Across the Spider-Verse",
                Genre = "Animation",
                Description = "Miles Morales voyage entre les dimensions du Spider-Verse.",
                SeasonsCount = 1,
                TotalEpisodes = 1,
                WatchedEpisodes = 0,
                Status = SerieStatus.Watchlist,
                PersonalRating = 8.7,
                StreamingPlatform = "Netflix",
                DateAdded = today.AddDays(-4),
                ContentType = ContentType.Film,
                PosterUrl = "https://image.tmdb.org/t/p/w500/8Vt6mWEReuy4Of61Lnj5Xj704m8.jpg",
                BackdropUrl = "https://image.tmdb.org/t/p/original/4HodYYKEIsGOdinkGi2Ucz6X9i0.jpg",
                ReleaseDate = new DateTime(2023, 6, 2),
                AverageEpisodeRuntimeMinutes = 140,
                LastWatchedSeason = 0,
                LastWatchedEpisode = 0,
                OwnerId = ownerId
            },
            new Serie
            {
                Title = "Your Name",
                Genre = "Anime",
                Description = "Deux adolescents echanges a distance cherchent a se retrouver.",
                SeasonsCount = 1,
                TotalEpisodes = 1,
                WatchedEpisodes = 0,
                Status = SerieStatus.Watchlist,
                PersonalRating = 8.6,
                StreamingPlatform = "Crunchyroll",
                DateAdded = today.AddDays(-3),
                ContentType = ContentType.Film,
                PosterUrl = "https://image.tmdb.org/t/p/w500/q719jXXEzOoYaps6babgKnONONX.jpg",
                BackdropUrl = "https://image.tmdb.org/t/p/original/mMtUybQ6hL24FXo0F3Z4j2KG7kZ.jpg",
                ReleaseDate = new DateTime(2016, 8, 26),
                AverageEpisodeRuntimeMinutes = 112,
                LastWatchedSeason = 0,
                LastWatchedEpisode = 0,
                OwnerId = ownerId
            }
        ];
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

