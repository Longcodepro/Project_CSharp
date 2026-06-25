using TuneVault.Domain.Enums;
using TuneVault.Application.Features.Notification.Commands;

namespace TuneVault.Application.Features.Share.Commands.ShareMedia;

/// <summary>
/// Repository command phục vụ luồng chia sẻ media, album và playlist.
/// </summary>
public interface IMediaShareCommandRepository
{
    /// <summary>
    /// Tìm share đã tồn tại cho cùng sender, receiver, item và loại chia sẻ.
    /// Dùng để giữ thao tác share idempotent, tránh tạo dữ liệu trùng vô hạn.
    /// </summary>
    Task<string?> FindExistingShareIdAsync(
        string senderId,
        string receiverId,
        ShareType shareType,
        string sharedItemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tạo bản ghi chia sẻ và notification trong cùng một transaction.
    /// </summary>
    Task<(string ShareId, string NotificationId)> CreateMediaShareWithNotificationAsync(
        string senderId,
        string receiverId,
        ShareType shareType,
        string sharedItemId,
        string? message,
        NotificationInsertModel notification,
        CancellationToken cancellationToken = default);

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
