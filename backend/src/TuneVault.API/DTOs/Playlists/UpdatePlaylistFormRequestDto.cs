namespace TuneVault.API.DTOs.Playlists;

/// <summary>
/// Form multipart dùng để cập nhật playlist kèm ảnh bìa mới nếu có.
/// </summary>
public sealed class UpdatePlaylistFormRequestDto
{
    /// <summary>Tiêu đề playlist.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Mô tả playlist.</summary>
    public string? Description { get; init; }

    /// <summary>Playlist có công khai hay không.</summary>
    public bool IsPublic { get; init; } = true;

    /// <summary>File ảnh bìa playlist mới.</summary>
    public IFormFile? CoverImage { get; init; }

    /// <summary>Giữ ảnh bìa hiện tại nếu không gửi file mới.</summary>
    public bool KeepCurrentCover { get; init; } = true;

    /// <summary>Kiểu nội dung chung của playlist.</summary>
    public string? ContentType { get; init; }

    /// <summary>Ngày phát hành playlist.</summary>
    public DateTime? ReleaseDate { get; init; }
}
