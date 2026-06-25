namespace TuneVault.Domain.Enums;

/// <summary>
/// Định nghĩa loại mục nội dung phương tiện được chia sẻ giữa bạn bè.
/// </summary>
public enum ShareType
{
    /// <summary>
    /// Chia sẻ một bài hát hoặc vật phẩm phương tiện đơn lẻ.
    /// </summary>
    MediaItem = 1,

    /// <summary>
    /// Chia sẻ một Album nhạc.
    /// </summary>
    Album = 2,

    /// <summary>
    /// Chia sẻ một Danh sách phát (Playlist).
    /// </summary>
    Playlist = 3
}
