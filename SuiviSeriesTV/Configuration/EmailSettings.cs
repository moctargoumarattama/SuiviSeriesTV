namespace SuiviSeriesTV.Configuration;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public bool Enabled { get; set; }
    public string SenderName { get; set; } = "SuiviSeriesTV";
    public string SenderEmail { get; set; } = "no-reply@suiviseriestv.local";
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = true;
}
