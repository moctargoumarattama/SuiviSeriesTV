using Microsoft.AspNetCore.Http;

namespace SuiviSeriesTV.Services.Storage;

public interface IFileStorageService
{
    Task<string?> SavePosterAsync(IFormFile? file, CancellationToken cancellationToken = default);
}
