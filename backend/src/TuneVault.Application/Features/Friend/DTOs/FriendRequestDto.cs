namespace TuneVault.Application.Features.Friend.DTOs;

/// <summary>
/// DTO trả về một lời mời kết bạn.
/// </summary>
public sealed record FriendRequestDto(
    string RequestId,
    string UserId,
    string IdDisplay,
    string DisplayName,
    string? AvatarUrl,
    DateTime RequestedAt,
    string Direction);
