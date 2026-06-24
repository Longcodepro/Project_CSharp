namespace TuneVault.API.DTOs.Media;

/// <summary>
/// Form multipart dùng để cập nhật metadata media và thay ảnh bìa/canvas bằng file trong wwwroot.
/// </summary>
public sealed class UpdateMediaFormRequestDto
{
    /// <summary>Tiêu đề mới.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Mô tả mới.</summary>
    public string? Description { get; init; }

    /// <summary>Thể loại mới.</summary>
    public string? Genre { get; init; }

    /// <summary>File audio mới, dùng cho audio/song/podcast.</summary>
    public IFormFile? AudioFile { get; init; }

    /// <summary>File video mới, dùng cho video.</summary>
    public IFormFile? VideoFile { get; init; }

    /// <summary>Ảnh bìa mới của media.</summary>
    public IFormFile? CoverImage { get; init; }

    /// <summary>Canvas video mới, chỉ dùng cho audio/song.</summary>
    public IFormFile? CanvasFile { get; init; }

    /// <summary>Media có công khai hay không.</summary>
    public bool IsPublic { get; init; } = true;

    /// <summary>Cấp độ truy cập: Normal hoặc Premium.</summary>
    public int AccessLevel { get; init; }
}
