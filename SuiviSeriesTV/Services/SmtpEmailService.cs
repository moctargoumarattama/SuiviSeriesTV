using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using SuiviSeriesTV.Configuration;

namespace SuiviSeriesTV.Services;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IOptions<EmailSettings> settings,
        IWebHostEnvironment environment,
        ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        if (_settings.Enabled && !string.IsNullOrWhiteSpace(_settings.SmtpHost))
        {
            using var smtpClient = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.UseSsl,
                Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = htmlContent,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);
            await smtpClient.SendMailAsync(message);
            return;
        }

        var emailDirectory = Path.Combine(_environment.ContentRootPath, "App_Data", "Emails");
        Directory.CreateDirectory(emailDirectory);

        var safeName = string.Join("_", toEmail.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{safeName}.html";
        var filePath = Path.Combine(emailDirectory, fileName);

        var builder = new StringBuilder();
        builder.AppendLine("<html><body style=\"font-family:Arial,sans-serif;\">");
        builder.AppendLine($"<h2>{subject}</h2>");
        builder.AppendLine(htmlContent);
        builder.AppendLine("<hr/>");
        builder.AppendLine("<p><em>SMTP disabled: this email was written locally for development.</em></p>");
        builder.AppendLine("</body></html>");

        await File.WriteAllTextAsync(filePath, builder.ToString());
        _logger.LogInformation("Email captured locally for {Recipient} at {FilePath}", toEmail, filePath);
    }
}
