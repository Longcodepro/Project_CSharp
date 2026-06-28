namespace TuneVault.Application.DTOs.Search;

/// <summary>
/// Thông tin media trong kết quả tìm kiếm.
/// </summary>
public sealed record SearchMediaResultDto(
    string Id,
    string Title,
    string? ArtistName,
    string? Genre,
    int DurationSeconds,
    int ViewCount,
    string? CoverImageUrl);
