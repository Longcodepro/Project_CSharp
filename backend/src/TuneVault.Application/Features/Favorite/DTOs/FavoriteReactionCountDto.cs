using TuneVault.Domain.Enums;

namespace TuneVault.Application.Features.Favorite.DTOs;

/// <summary>
/// Số lượt thể hiện cảm xúc hiện có của một media, album hoặc playlist.
/// </summary>
public sealed record FavoriteReactionCountDto(
    string TargetId,
    FavoriteTargetType TargetType,
    int TotalCount);
