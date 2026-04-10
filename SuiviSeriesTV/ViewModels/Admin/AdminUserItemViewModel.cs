namespace SuiviSeriesTV.ViewModels.Admin;

public class AdminUserItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsLocked { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
