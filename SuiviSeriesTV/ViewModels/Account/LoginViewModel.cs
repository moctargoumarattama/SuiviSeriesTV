using System.ComponentModel.DataAnnotations;

namespace SuiviSeriesTV.ViewModels.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "L'email est obligatoire.")]
    [EmailAddress(ErrorMessage = "Format d'email invalide.")]
    [Display(Name = "Adresse email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Se souvenir de moi")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
