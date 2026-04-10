using System.ComponentModel.DataAnnotations;

namespace SuiviSeriesTV.ViewModels.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "L'email est obligatoire.")]
    [EmailAddress(ErrorMessage = "Format d'email invalide.")]
    [Display(Name = "Adresse email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caracteres.")]
    [Display(Name = "Mot de passe")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmation du mot de passe est obligatoire.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "La confirmation ne correspond pas au mot de passe.")]
    [Display(Name = "Confirmer le mot de passe")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
