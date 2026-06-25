namespace TuneVault.Application.Features.Friend.DTOs;

/// <summary>
/// DTO trả về một người bạn trong danh sách bạn bè.
/// </summary>
public sealed record FriendDto(
    string UserId,
    string IdDisplay,
    string DisplayName,
    string? AvatarUrl,
    DateTime FriendsSince);
