using TuneVault.Domain.Enums;

namespace TuneVault.Application.Features.Friend.Abstractions;

/// <summary>
/// Ảnh chụp một bản ghi quan hệ bạn bè hoặc lời mời kết bạn trong database.
/// </summary>
public sealed record FriendRelationSnapshot(
    string Id,
    string RequestedById,
    string RequestedToId,
    byte StatusRaw,
    DateTime CreatedAt)
{
    public FriendStatus Status => (FriendStatus)StatusRaw;
}

/// <summary>
/// Dữ liệu hiển thị một người bạn trong danh sách bạn bè.
/// </summary>
public sealed record FriendListItem(
    string UserId,
    string IdDisplay,
    string DisplayName,
    string? AvatarUrl,
    DateTime FriendsSince);

/// <summary>
/// Dữ liệu hiển thị một lời mời kết bạn trong inbox hoặc sent.
/// </summary>
public sealed record FriendRequestItem(
    string RequestId,
    string UserId,
    string IdDisplay,
    string DisplayName,
    string? AvatarUrl,
    DateTime RequestedAt,
    string Direction);

/// <summary>
/// Repository phục vụ feature kết bạn.
/// </summary>
public interface IFriendRepository
{
    /// <summary>
    /// Kiểm tra tài khoản có tồn tại và còn hoạt động hay không.
    /// </summary>
    Task<bool> UserExistsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy một bản ghi lời mời hoặc quan hệ bạn bè theo mã.
    /// </summary>
    Task<FriendRelationSnapshot?> GetByIdAsync(string requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy quan hệ hiện có giữa hai người dùng, không phân biệt chiều.
    /// </summary>
    Task<FriendRelationSnapshot?> GetRelationshipAsync(string firstUserId, string secondUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tạo lời mời kết bạn mới.
    /// </summary>
    Task<string> CreateRequestAsync(string requestedById, string requestedToId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Chấp nhận lời mời đang chờ xử lý.
    /// </summary>
    Task AcceptRequestAsync(string requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hủy lời mời đang chờ xử lý bằng cách chuyển trạng thái hoạt động.
    /// </summary>
    Task DeletePendingRequestAsync(string requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hủy quan hệ bạn bè đã được chấp nhận bằng cách chuyển trạng thái hoạt động.
    /// </summary>
    Task DeleteAcceptedFriendshipAsync(string currentUserId, string friendUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách bạn bè hiện tại.
    /// </summary>
    Task<IReadOnlyCollection<FriendListItem>> GetFriendsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách lời mời người dùng hiện tại nhận được.
    /// </summary>
    Task<IReadOnlyCollection<FriendRequestItem>> GetIncomingRequestsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách lời mời người dùng hiện tại đã gửi.
    /// </summary>
    Task<IReadOnlyCollection<FriendRequestItem>> GetSentRequestsAsync(string userId, CancellationToken cancellationToken = default);
}
