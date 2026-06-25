using TuneVault.Domain.Enums;

namespace TuneVault.Application.Features.CollectionLike.DTOs;

/// <summary>
/// DTO trả về album/playlist người dùng đã thích.
/// </summary>
/// <param name="Id">Mã lượt thích.</param>
/// <param name="TargetId">Mã album hoặc playlist.</param>
/// <param name="TargetType">Loại đối tượng được thích.</param>
/// <param name="Title">Tên album hoặc playlist.</param>
/// <param name="Description">Mô tả ngắn nếu có.</param>
/// <param name="CoverImageUrl">Ảnh bìa nếu có.</param>
/// <param name="LikedAt">Thời điểm bấm thích.</param>
public sealed record CollectionLikeDto(
    string Id,
    string TargetId,
    CollectionLikeTargetType TargetType,
    string Title,
    string? Description,
    string? CoverImageUrl,
    DateTime LikedAt);
