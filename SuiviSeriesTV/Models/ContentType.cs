using System.ComponentModel.DataAnnotations;

namespace SuiviSeriesTV.Models;

public enum ContentType
{
    [Display(Name = "Serie")]
    Serie = 0,

    [Display(Name = "Film")]
    Film = 1,

    [Display(Name = "Anime")]
    Anime = 2
}
