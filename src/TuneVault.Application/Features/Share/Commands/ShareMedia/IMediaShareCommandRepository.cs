namespace TuneVault.Application.Features.Share.Commands.ShareMedia;

/// <summary>
/// Repository command phục vụ luồng chia sẻ media, album và playlist.
/// </summary>
public interface IMediaShareCommandRepository
{
    /// <summary>
    /// Tạo bản ghi chia sẻ.
    /// </summary>
    Task<string> CreateMediaShareAsync(string senderId, string receiverId, string shareType, string sharedItemId, string? message);

    /// <summary>
    /// Kiểm tra người dùng nhận chia sẻ còn hoạt động.
    /// </summary>
    Task<bool> UserExistsAsync(string userId);

    /// <summary>
    /// Kiểm tra media có thể share bởi user hiện tại.
    /// </summary>
    Task<bool> TrackExistsAsync(string mediaItemId, string senderId);

    /// <summary>
    /// Kiểm tra album có thể share bởi user hiện tại.
    /// </summary>
    Task<bool> AlbumExistsAsync(string albumId, string senderId);

    /// <summary>
    /// Kiểm tra playlist có thể share bởi user hiện tại.
    /// </summary>
    Task<bool> PlaylistExistsAsync(string playlistId, string senderId);
}
