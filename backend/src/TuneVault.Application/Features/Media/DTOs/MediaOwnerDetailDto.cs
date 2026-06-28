namespace TuneVault.Application.Features.Media.DTOs;

/// <summary>
/// DTO chi tiết dành cho owner khi xem danh sách media của chính mình.
/// Bao gồm thêm các cờ quản trị và metadata phục vụ chỉnh sửa.
/// </summary>
public sealed record MediaOwnerDetailDto(
    string Id,
    string OwnerId,
    string? OwnerName,
    string Title,
    string? Description,
    string? Genre,
    string Type,
    string? AudioUrl,
    string? VideoUrl,
    string? CoverImageUrl,
    string? CanvasUrl,
    int DurationSeconds,
    bool IsPublic,
    bool IsActive,
    int FavoriteCount,
    int ViewCount,
    DateTime UploadedAt,
    DateTime? ReleaseDate
);
