using Microsoft.AspNetCore.Identity;

namespace SuiviSeriesTV.Models;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
