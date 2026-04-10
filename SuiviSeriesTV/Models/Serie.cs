using System.ComponentModel.DataAnnotations;

namespace SuiviSeriesTV.Models;

public class Serie : IValidatableObject
{
    public int Id { get; set; }

    [Display(Name = "Type")]
    public ContentType ContentType { get; set; } = ContentType.Serie;

    [Required(ErrorMessage = "Le titre est obligatoire.")]
    [StringLength(120, ErrorMessage = "Le titre ne doit pas depasser 120 caracteres.")]
    [Display(Name = "Titre")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le genre est obligatoire.")]
    [StringLength(60, ErrorMessage = "Le genre ne doit pas depasser 60 caracteres.")]
    [Display(Name = "Genre")]
    public string Genre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La description est obligatoire.")]
    [StringLength(1000, ErrorMessage = "La description ne doit pas depasser 1000 caracteres.")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Range(1, 200, ErrorMessage = "Le nombre de saisons doit etre entre 1 et 200.")]
    [Display(Name = "Nombre de saisons")]
    public int SeasonsCount { get; set; } = 1;

    [Range(1, 100000, ErrorMessage = "Le nombre total d'episodes doit etre superieur a 0.")]
    [Display(Name = "Nombre total d'episodes")]
    public int TotalEpisodes { get; set; }

    [Range(0, 100000, ErrorMessage = "Le nombre d'episodes vus doit etre positif.")]
    [Display(Name = "Nombre d'episodes vus")]
    public int WatchedEpisodes { get; set; }

    [Display(Name = "Statut")]
    public SerieStatus Status { get; set; } = SerieStatus.Watchlist;

    [Range(0, 10, ErrorMessage = "La note doit etre comprise entre 0 et 10.")]
    [Display(Name = "Note personnelle sur 10")]
    public double PersonalRating { get; set; }

    [Required(ErrorMessage = "La plateforme de streaming est obligatoire.")]
    [StringLength(80, ErrorMessage = "Le nom de la plateforme ne doit pas depasser 80 caracteres.")]
    [Display(Name = "Plateforme de streaming")]
    public string StreamingPlatform { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Date d'ajout")]
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;

    [Url(ErrorMessage = "L'URL de l'affiche n'est pas valide.")]
    [StringLength(400, ErrorMessage = "L'URL de l'affiche ne doit pas depasser 400 caracteres.")]
    [Display(Name = "Image / Affiche URL")]
    public string? PosterUrl { get; set; }

    [Url(ErrorMessage = "L'URL du backdrop n'est pas valide.")]
    [StringLength(400, ErrorMessage = "L'URL du backdrop ne doit pas depasser 400 caracteres.")]
    [Display(Name = "Image de fond URL")]
    public string? BackdropUrl { get; set; }

    [Display(Name = "Sortie/Prochaine diffusion")]
    [DataType(DataType.Date)]
    public DateTime? NextReleaseDate { get; set; }

    [Display(Name = "Date de sortie")]
    [DataType(DataType.Date)]
    public DateTime? ReleaseDate { get; set; }

    [Range(0, 10000, ErrorMessage = "Le numero de saison doit etre positif.")]
    [Display(Name = "Derniere saison vue")]
    public int LastWatchedSeason { get; set; }

    [Range(0, 100000, ErrorMessage = "Le numero d'episode doit etre positif.")]
    [Display(Name = "Dernier episode vu")]
    public int LastWatchedEpisode { get; set; }

    [Range(1, 500, ErrorMessage = "La duree moyenne doit etre entre 1 et 500 minutes.")]
    [Display(Name = "Duree moyenne (min)")]
    public int? AverageEpisodeRuntimeMinutes { get; set; }

    [Display(Name = "Favori")]
    public bool IsFavorite { get; set; }

    [StringLength(1200, ErrorMessage = "Le commentaire ne doit pas depasser 1200 caracteres.")]
    [Display(Name = "Commentaire personnel")]
    public string? PersonalComment { get; set; }

    [Display(Name = "Identifiant TMDB")]
    public int? TmdbId { get; set; }

    public string? OwnerId { get; set; }
    public ApplicationUser? Owner { get; set; }

    public int ProgressPercentage => TotalEpisodes <= 0
        ? 0
        : (int)Math.Clamp(Math.Round((double)WatchedEpisodes * 100 / TotalEpisodes), 0, 100);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (WatchedEpisodes > TotalEpisodes)
        {
            yield return new ValidationResult(
                "Le nombre d'episodes vus ne peut pas depasser le nombre total d'episodes.",
                new[] { nameof(WatchedEpisodes), nameof(TotalEpisodes) });
        }

        if (ContentType == ContentType.Film && SeasonsCount != 1)
        {
            yield return new ValidationResult(
                "Un film doit avoir exactement 1 saison (format unitaire).",
                new[] { nameof(SeasonsCount), nameof(ContentType) });
        }

        if (LastWatchedEpisode > WatchedEpisodes)
        {
            yield return new ValidationResult(
                "Le dernier episode vu ne peut pas depasser le nombre d'episodes vus.",
                new[] { nameof(LastWatchedEpisode), nameof(WatchedEpisodes) });
        }
    }
}

