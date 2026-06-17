using Microsoft.AspNetCore.Http;

namespace TuneVault.Application.Abstractions;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<string> SaveAsync(IFormFile file, string subfolder, CancellationToken cancellationToken = default);
    Task DeleteAsync(string filePath, CancellationToken cancellationToken = default);
}
