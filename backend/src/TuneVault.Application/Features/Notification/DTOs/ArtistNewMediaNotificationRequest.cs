namespace TuneVault.Application.Features.Notification.DTOs;

/// <summary>
/// Request body dùng để tạo thông báo demo khi nghệ sĩ đăng bài mới.
/// </summary>
public sealed record ArtistNewMediaNotificationRequest(
    string UserId,
    string ArtistId,
    string MediaItemId,
    string Title);