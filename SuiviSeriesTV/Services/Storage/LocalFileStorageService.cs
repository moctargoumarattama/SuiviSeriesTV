using Microsoft.AspNetCore.Http;

namespace SuiviSeriesTV.Services.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> SavePosterAsync(IFormFile? file, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length <= 0)
        {
            return null;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return null;
        }

        var directory = Path.Combine(_environment.WebRootPath, "uploads", "posters");
        Directory.CreateDirectory(directory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(directory, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/uploads/posters/{fileName}";
    }
}
