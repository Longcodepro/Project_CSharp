namespace TuneVault.Application.DTOs.Search;

/// <summary>
/// DTO - SEARCH RESULT (Application Layer)
/// ======================================
/// Mục đích: Container chứa tất cả kết quả tìm kiếm từ SearchController endpoint.
/// 
/// Sử dụng:
/// - SearchController.Search() -> SearchResultDto
/// - Trả về duy nhất 1 response object cho client
/// 
/// Tính chất: Record (immutable, value-based equality)
/// Properties:
///   - Media: Danh sách bài hát/podcast tìm được (SearchMediaResultDto[])
///   - Artists: Danh sách nghệ sĩ tìm được (SearchArtistResultDto[])
///   - Playlists: Danh sách playlist tìm được (SearchPlaylistResultDto[])
///   - TrendingMedia: Top bài nghe nhiều nhất (SearchMediaResultDto[])
///   - TotalCount: Tổng số kết quả (Media.Count + Artists.Count + Playlists.Count)
/// 
/// Ví dụ Response JSON:
/// {
///   "media": [{...}, {...}],
///   "artists": [{...}],
///   "playlists": [{...}],
///   "trendingMedia": [{...}, {...}, {...}],
///   "totalCount": 5
/// }
/// </summary>

public sealed record SearchResultDto(
    IReadOnlyCollection<SearchMediaResultDto>? Media,
    IReadOnlyCollection<SearchArtistResultDto>? Artists,
    IReadOnlyCollection<SearchPlaylistResultDto>? Playlists,
    IReadOnlyCollection<SearchMediaResultDto>? TrendingMedia,
    int TotalCount);
