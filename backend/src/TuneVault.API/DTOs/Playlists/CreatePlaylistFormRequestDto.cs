namespace TuneVault.API.DTOs.Playlists;

/// <summary>
/// Form multipart dùng để tạo playlist kèm ảnh bìa lưu trong wwwroot.
/// </summary>
public sealed class CreatePlaylistFormRequestDto
{
    /// <summary>Tiêu đề playlist.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Mô tả playlist.</summary>
    public string? Description { get; init; }

    /// <summary>Playlist có công khai hay không.</summary>
    public bool IsPublic { get; init; } = true;

    /// <summary>File ảnh bìa playlist.</summary>
    public IFormFile? CoverImage { get; init; }

    /// <summary>Kiểu nội dung chung của playlist.</summary>
    public string? ContentType { get; init; }

    /// <summary>Ngày phát hành playlist.</summary>
    public DateTime? ReleaseDate { get; init; }
}
