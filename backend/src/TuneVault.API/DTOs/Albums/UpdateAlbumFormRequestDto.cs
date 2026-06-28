namespace TuneVault.API.DTOs.Albums;

/// <summary>
/// Form multipart dùng để cập nhật album kèm ảnh bìa mới nếu có.
/// </summary>
public sealed class UpdateAlbumFormRequestDto
{
    /// <summary>Tiêu đề album.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Mô tả album.</summary>
    public string? Description { get; init; }

    /// <summary>File ảnh bìa album mới.</summary>
    public IFormFile? CoverImage { get; init; }

    /// <summary>URL ảnh bìa mặc định trong /uploads/default-cover.</summary>
    public string? CoverImageUrl { get; init; }

    /// <summary>Giữ ảnh bìa hiện tại nếu không gửi file mới.</summary>
    public bool KeepCurrentCover { get; init; } = true;

    /// <summary>Album có công khai hay không.</summary>
    public bool IsPublic { get; init; } = true;

    /// <summary>Kiểu nội dung chung của album.</summary>
    public string? ContentType { get; init; }

    /// <summary>Ngày phát hành album.</summary>
    public DateTime? ReleaseDate { get; init; }
}
