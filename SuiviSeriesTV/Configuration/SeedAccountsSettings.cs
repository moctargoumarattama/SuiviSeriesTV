namespace SuiviSeriesTV.Configuration;

public class SeedAccountsSettings
{
    public const string SectionName = "SeedAccounts";

    public string AdminEmail { get; set; } = "admin@suiviseriestv.local";
    public string AdminPassword { get; set; } = "Admin@12345";
    public string UserEmail { get; set; } = "user@suiviseriestv.local";
    public string UserPassword { get; set; } = "User@12345";
}
