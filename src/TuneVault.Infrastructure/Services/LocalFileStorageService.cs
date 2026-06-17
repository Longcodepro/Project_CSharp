using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using TuneVault.Application.Abstractions;

namespace TuneVault.Infrastructure.Services;

/// <summary>
/// Lưu file media vào thư mục uploads nằm trong project API để tránh mất file khi hệ thống dọn /tmp.
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private static readonly HashSet<string> AudioExtensions = [".mp3", ".wav", ".m4a", ".flac", ".ogg"];
    private static readonly HashSet<string> VideoExtensions = [".mp4", ".webm"];
    private static readonly HashSet<string> ImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    /// <summary>
    /// Khởi tạo service với môi trường host để xác định thư mục gốc của API.
    /// </summary>
    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    public Task<string> SaveAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var folderPath = GetUploadFolderPath(ResolveFolder(null, fileName, contentType));
        Directory.CreateDirectory(folderPath);

        var safeName = BuildSafeFileName(fileName);
        var fullPath = Path.Combine(folderPath, safeName);

        return SaveStreamAsync(stream, fullPath, cancellationToken);
    }

    public async Task<string> SaveAsync(IFormFile file, string subfolder, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("File không được rỗng.", nameof(file));

        var folderPath = GetUploadFolderPath(ResolveFolder(subfolder, file.FileName, file.ContentType));
        Directory.CreateDirectory(folderPath);

        var safeName = BuildSafeFileName(file.FileName);
        var fullPath = Path.Combine(folderPath, safeName);

        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream, cancellationToken);
        return fullPath;
    }

    /// <summary>
    /// Trả về thư mục upload theo từng loại file trong project API.
    /// </summary>
    private string GetUploadFolderPath(string? subfolder)
    {
        var safeSubfolder = string.IsNullOrWhiteSpace(subfolder) ? "misc" : subfolder.Trim();
        return Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", safeSubfolder);
    }

    /// <summary>
    /// Chuẩn hóa thư mục lưu trữ để các loại upload không bị trộn lẫn trong wwwroot/uploads.
    /// </summary>
    private static string ResolveFolder(string? requestedSubfolder, string fileName, string? contentType)
    {
        var requested = requestedSubfolder?.Trim().ToLowerInvariant();

        return requested switch
        {
            "audio" => "audio",
            "video" => "video",
            "avatar" or "avatars" or "user-avatar" or "user-avatars" => "avatars",
            "media-cover" or "media-covers" or "poster" or "posters" or "cover" or "covers" => "media-covers",
            "album-cover" or "album-covers" => "album-covers",
            "playlist-cover" or "playlist-covers" => "playlist-covers",
            "misc" => "misc",
            _ => InferFolder(fileName, contentType)
        };
    }

    /// <summary>
    /// Khi caller không nói rõ loại media, chỉ tự phân loại các loại chắc chắn; còn lại đưa vào misc.
    /// </summary>
    private static string InferFolder(string fileName, string? contentType)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var normalizedContentType = contentType?.Trim().ToLowerInvariant() ?? string.Empty;

        if (AudioExtensions.Contains(extension) || normalizedContentType.StartsWith("audio/"))
            return "audio";

        if (VideoExtensions.Contains(extension) || normalizedContentType.StartsWith("video/"))
            return "video";

        if (ImageExtensions.Contains(extension) || normalizedContentType.StartsWith("image/"))
            return "misc";

        return "misc";
    }

    private static async Task<string> SaveStreamAsync(Stream stream, string fullPath, CancellationToken ct)
    {
        await using var output = File.Create(fullPath);
        await stream.CopyToAsync(output, ct);
        return fullPath;
    }

    /// <summary>
    /// Tạo tên file chỉ gồm GUID và extension ASCII để path lưu trong cột varchar không bị lỗi Unicode.
    /// </summary>
    private static string BuildSafeFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return $"{Guid.NewGuid():N}{extension}";
    }
}
