using TuneVault.Application.Abstractions;

namespace TuneVault.Infrastructure.Services;

public sealed class LocalFileStorageService : IFileStorageService
{
    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Implement file deletion storage logic here.");
    }

    public Task<string> SaveAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Implement file storage logic here.");
    }
}
