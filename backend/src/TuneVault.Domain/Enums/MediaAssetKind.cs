namespace TuneVault.Domain.Enums;

/// <summary>
/// Loại asset vật lý được dùng để phát hoặc hiển thị cho một media item.
/// </summary>
public enum MediaAssetKind
{
    /// <summary>
    /// Asset chính của media: audio cho bài hát/podcast, video cho video.
    /// </summary>
    Primary,

    /// <summary>
    /// File audio dùng cho trình phát nhạc.
    /// </summary>
    Audio,

    /// <summary>
    /// File video dùng cho trình phát video.
    /// </summary>
    Video,

    /// <summary>
    /// Ảnh poster/thumbnail/cover của media.
    /// </summary>
    Poster
}
