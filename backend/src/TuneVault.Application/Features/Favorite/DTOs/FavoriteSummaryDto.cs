namespace TuneVault.Application.Features.Favorite.DTOs;

/// <summary>
/// DTO gọn cho danh sách nội dung người dùng đã yêu thích.
/// </summary>
/// <param name="Title">Tên bài hát hoặc media.</param>
public sealed record FavoriteSummaryDto(string Title);
