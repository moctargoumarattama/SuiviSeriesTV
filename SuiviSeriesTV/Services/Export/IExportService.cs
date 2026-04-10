namespace SuiviSeriesTV.Services.Export;

public interface IExportService
{
    Task<byte[]> BuildLibraryCsvAsync(string userId, bool isAdmin, CancellationToken cancellationToken = default);
}
