namespace TuneVault.Application.Features.Media.DTOs;

/// <summary>
/// DTO gọn dùng cho người xem công khai khi duyệt danh sách media.
/// Không chứa các cờ quản trị hoặc dữ liệu chỉ cần cho owner.
/// </summary>
public sealed record MediaPublicDto(
    string Id,
    string Title,
    string? Description,
    string? Genre,
    string Type,
    string? AudioUrl,
    string? VideoUrl,
    string? CoverImageUrl,
    int FavoriteCount,
    int ViewCount,
    DateTime UploadedAt,
    DateTime? ReleaseDate,
    IEnumerable<MediaArtistDto> Artists
);
