namespace TuneVault.API.DTOs.Users;

/// <summary>
/// Dữ liệu request cho thao tác bỏ theo dõi một người dùng khác.
/// Backend luôn lấy người hủy theo dõi từ JWT để tránh thao tác thay người khác.
/// </summary>
public sealed class UnfollowUserRequestDto
{
    /// <summary>
    /// Mã người dùng bị bỏ theo dõi.
    /// </summary>
    public string FolloweeId { get; init; } = string.Empty;
}
