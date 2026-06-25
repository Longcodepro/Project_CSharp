namespace TuneVault.API.DTOs.Users;

/// <summary>
/// Dữ liệu request cho thao tác theo dõi một người dùng khác.
/// Backend luôn lấy người theo dõi từ JWT để tránh giả mạo.
/// </summary>
public sealed class FollowUserRequestDto
{
    /// <summary>
    /// Mã người dùng được theo dõi.
    /// </summary>
    public string FolloweeId { get; init; } = string.Empty;
}
