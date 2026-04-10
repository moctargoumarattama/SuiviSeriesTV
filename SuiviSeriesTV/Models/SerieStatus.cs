using System.ComponentModel.DataAnnotations;

namespace SuiviSeriesTV.Models;

public enum SerieStatus
{
    [Display(Name = "A voir")]
    Watchlist = 0,

    [Display(Name = "En cours")]
    EnCours = 1,

    [Display(Name = "Termine")]
    Termine = 2,

    [Display(Name = "Abandonne")]
    Abandonne = 3
}

