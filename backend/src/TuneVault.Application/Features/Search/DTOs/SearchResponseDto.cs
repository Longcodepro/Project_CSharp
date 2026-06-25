namespace TuneVault.Application.DTOs.Search;

/// <summary>
/// DTO - SEARCH RESPONSE (Application Layer)
/// ==========================================
/// Mục đích: Wrap toàn bộ kết quả tìm kiếm kèm thông tin phân trang.
/// 
/// Sử dụng:
/// - SearchMediaQueryHandler.HandleAsync() -> SearchResponseDto
/// - SearchController.Search() -> trả về SearchResponseDto cho client
/// 
/// Properties:
///   - Data: Kết quả tìm kiếm (SearchResultDto)
///   - Page: Trang hiện tại
///   - PageSize: Số kết quả mỗi trang
///   - TotalMedia: Tổng số bài hát tìm được
///   - TotalPages: Tổng số trang
/// </summary>
public sealed record SearchResponseDto(
    SearchResultDto Data,
    int Page,
    int PageSize,
    int TotalMedia,
    int TotalPages);
