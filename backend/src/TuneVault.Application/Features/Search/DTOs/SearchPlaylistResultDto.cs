namespace TuneVault.Application.DTOs.Search;

/// <summary>
/// Thông tin playlist trong kết quả tìm kiếm.
/// </summary>
public sealed record SearchPlaylistResultDto(
    string Id,
    string Title,
    string? CoverImageUrl,
    string OwnerName,
    int TrackCount,
    DateTime CreatedAt);
