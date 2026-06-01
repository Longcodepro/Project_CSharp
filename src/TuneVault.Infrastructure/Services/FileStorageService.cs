using TuneVault.Application.Abstractions;

namespace TuneVault.Infrastructure.Services;

public sealed class FileStorageService : IFileStorageService
{
    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<string> SaveAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
