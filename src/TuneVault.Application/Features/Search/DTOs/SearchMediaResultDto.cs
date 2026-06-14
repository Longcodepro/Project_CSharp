namespace TuneVault.Application.DTOs.Search;

/// <summary>
/// DTO - SEARCH MEDIA RESULT (Application Layer)
/// =============================================
/// Mục đích: Đại diện cho một bài hát/podcast trong kết quả tìm kiếm.
/// 
/// Sử dụng:
/// - SearchRepository.SearchMediaAsync() -> dynamic -> SearchMediaResultDto
/// - SearchRepository.GetTrendingAsync() -> dynamic -> SearchMediaResultDto
/// - Được trả về trong SearchResultDto.Media[] và SearchResultDto.TrendingMedia[]
/// 
/// Tính chất: Record (immutable, value-based equality)
/// Properties:
///   - Id: Mã định danh bài hát
///   - Title: Tên bài hát
///   - ArtistName: Tên nghệ sĩ
///   - Genre: Thể loại
///   - DurationSeconds: Thời lượng (giây)
///   - ViewCount: Số lượt nghe
///   - CoverImageUrl: URL ảnh bìa
/// </summary>

public sealed record SearchMediaResultDto(
    string Id,
    string Title,
    string? ArtistName,
    string? Genre,
    int DurationSeconds,
    int ViewCount,
    string? CoverImageUrl);
