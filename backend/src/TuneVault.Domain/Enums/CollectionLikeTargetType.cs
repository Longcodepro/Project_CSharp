namespace TuneVault.Domain.Enums;

/// <summary>
/// Phân loại đối tượng được người dùng thích trong nhóm album/playlist.
/// </summary>
public enum CollectionLikeTargetType
{
    /// <summary>
    /// Người dùng thích một album.
    /// </summary>
    Album = 1,

    /// <summary>
    /// Người dùng thích một playlist.
    /// </summary>
    Playlist = 2
}
