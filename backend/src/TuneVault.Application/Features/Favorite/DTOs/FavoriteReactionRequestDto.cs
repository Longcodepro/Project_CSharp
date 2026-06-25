using TuneVault.Domain.Enums;

namespace TuneVault.Application.Features.Favorite.DTOs;

/// <summary>
/// Request body dùng để chọn cảm xúc của người dùng với một media.
/// Nếu client không gửi body, controller sẽ mặc định là <see cref="FavoriteReaction.Like"/>.
/// </summary>
/// <param name="Reaction">Cảm xúc muốn lưu cho media.</param>
public sealed record FavoriteReactionRequestDto(FavoriteReaction Reaction = FavoriteReaction.Like);
