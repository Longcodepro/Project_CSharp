namespace TuneVault.Domain.Enums;

/// <summary>
/// Phân loại đối tượng mà người dùng thể hiện cảm xúc trong bảng Favorites.
/// </summary>
public enum FavoriteTargetType
{
    /// <summary>
    /// Cảm xúc dành cho media item/bài hát/video.
    /// </summary>
    Media = 0,

    /// <summary>
    /// Cảm xúc dành cho album.
    /// </summary>
    Album = 1,

    /// <summary>
    /// Cảm xúc dành cho playlist.
    /// </summary>
    Playlist = 2
}
