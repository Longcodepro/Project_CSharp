namespace TuneVault.API.DTOs.Albums;

/// <summary>
/// Form multipart dùng để tạo album kèm ảnh bìa lưu trong wwwroot.
/// </summary>
public sealed class CreateAlbumFormRequestDto
{
    /// <summary>Tiêu đề album.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Mô tả album.</summary>
    public string? Description { get; init; }

    /// <summary>File ảnh bìa album.</summary>
    public IFormFile? CoverImage { get; init; }

    /// <summary>URL ảnh bìa mặc định trong /uploads/default-cover.</summary>
    public string? CoverImageUrl { get; init; }

    /// <summary>Album có công khai hay không.</summary>
    public bool IsPublic { get; init; } = true;

    /// <summary>Kiểu nội dung chung của album.</summary>
    public string? ContentType { get; init; }

    /// <summary>Ngày phát hành album.</summary>
    public DateTime? ReleaseDate { get; init; }
}
