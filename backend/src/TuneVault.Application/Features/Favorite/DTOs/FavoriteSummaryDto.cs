using TuneVault.Domain.Enums;

namespace TuneVault.Application.Features.Favorite.DTOs;

/// <summary>
/// DTO gọn cho danh sách cảm xúc người dùng đã thể hiện với media.
/// Chỉ trả tên bài và loại cảm xúc để không làm response danh sách yêu thích bị quá dài.
/// </summary>
/// <param name="Title">Tên bài hát hoặc media.</param>
/// <param name="Reaction">Cảm xúc người dùng đã chọn.</param>
public sealed record FavoriteSummaryDto(string Title, FavoriteReaction Reaction);
