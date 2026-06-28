using Microsoft.AspNetCore.Http;

namespace TuneVault.Application.Features.Media.DTOs;

/// <summary>
/// Form multipart dùng cho upload media qua Swagger hoặc frontend.
/// </summary>
public sealed class UploadMediaFormDto
{
    public IFormFile? AudioFile { get; set; }
    public IFormFile? VideoFile { get; set; }
    public IFormFile? CoverImage { get; set; }
    public string? CoverImageUrl { get; set; }
    public IFormFile? CanvasFile { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Genre { get; set; }
    public string Type { get; set; } = "Audio";
    public int? DurationSeconds { get; set; }
    public bool IsPublic { get; set; } = true;
    public DateTime? ReleaseDate { get; set; }
}
