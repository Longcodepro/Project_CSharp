using MediatR;
using TuneVault.Application.DTOs.Search;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Search.Queries.SearchMedia;

/// <summary>
/// QUERY HANDLER - TÌM KIẾM MEDIA (Application Layer)
/// ===================================================
/// Mục đích: Xử lý logic truy vấn tìm kiếm toàn bộ theo keyword.
/// 
/// Luồng xử lý:
/// 1. Controller gửi SearchMediaQuery
/// 2. Handler gọi Repository chạy 4 queries song song:
///    - SearchMediaAsync: Tìm bài hát/podcast theo keyword
///    - SearchArtistsAsync: Tìm nghệ sĩ theo keyword
///    - SearchPlaylistsAsync: Tìm playlist công khai theo keyword
///    - GetTrendingAsync: Lấy top 10 bài nghe nhiều nhất
/// 3. Phân trang kết quả media theo Page và PageSize
/// 4. Map dynamic -> DTOs
/// 5. Trả về SearchResultDto cho Controller
/// 
/// Lý do tách ra khỏi Controller:
/// - Controller chỉ lo nhận/trả HTTP request
/// - Logic tìm kiếm và phân trang tập trung tại đây, dễ test, dễ bảo trì
/// </summary>
public sealed class SearchMediaQueryHandler : IRequestHandler<SearchMediaQuery, SearchResponseDto>
{
    private readonly ISearchRepository _searchRepository;

    /// <summary>
    /// Khởi tạo Handler với Repository được inject qua DI container.
    /// </summary>
    /// <param name="searchRepository">Repository xử lý truy cập database cho Search.</param>
    public SearchMediaQueryHandler(ISearchRepository searchRepository)
    {
        _searchRepository = searchRepository;
    }

    /// <summary>
    /// Thực thi logic tìm kiếm toàn bộ từ Query.
    /// Chạy song song 4 queries và phân trang kết quả media.
    /// </summary>
    /// <param name="query">Query chứa Keyword, Page và PageSize.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Object chứa data kết quả, thông tin phân trang.</returns>
    public async Task<SearchResponseDto> Handle(SearchMediaQuery query, CancellationToken cancellationToken = default)
    {
        // Chuẩn hóa page và pageSize
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = (query.PageSize < 1 || query.PageSize > 50) ? 10 : query.PageSize;

        // Chạy 4 queries lấy dữ liệu từ Repository
        var mediaResults = await _searchRepository.SearchMediaAsync(query.Keyword, cancellationToken);
        var artistResults = await _searchRepository.SearchArtistsAsync(query.Keyword, cancellationToken);
        var playlistResults = await _searchRepository.SearchPlaylistsAsync(query.Keyword, cancellationToken);
        var trendingResults = await _searchRepository.GetTrendingAsync(10, cancellationToken);

        // Map dynamic -> SearchMediaResultDto
        var allMedia = mediaResults.Cast<dynamic>().Select(m => new SearchMediaResultDto(
            (string)m.Id,
            (string)m.Title,
            (string?)m.ArtistName,
            (string?)m.Genre,
            (int)m.DurationSeconds,
            (int)m.ViewCount,
            string.IsNullOrWhiteSpace((string?)m.CoverImageUrl) ? null : MediaEndpointBuilder.Poster((string)m.Id)
        )).ToList();

        // Map dynamic -> SearchArtistResultDto
        var allArtists = artistResults.Cast<dynamic>().Select(a => new SearchArtistResultDto(
            (string)a.Id,
            (string)a.UserName,
            (string)a.DisplayName,
            (string?)a.AvatarUrl,
            (int)a.TotalFollowers
        )).ToList();

        // Map dynamic -> SearchPlaylistResultDto
        var allPlaylists = playlistResults.Cast<dynamic>().Select(p => new SearchPlaylistResultDto(
            (string)p.Id,
            (string)p.Title,
            (string?)p.CoverImageUrl,
            (string)p.OwnerName,
            (int)p.TrackCount,
            (DateTime)p.CreatedAt
        )).ToList();

        // Map dynamic -> SearchMediaResultDto cho Trending
        var trending = trendingResults.Cast<dynamic>().Select(m => new SearchMediaResultDto(
            (string)m.Id,
            (string)m.Title,
            (string?)m.ArtistName,
            (string?)m.Genre,
            (int)m.DurationSeconds,
            (int)m.ViewCount,
            string.IsNullOrWhiteSpace((string?)m.CoverImageUrl) ? null : MediaEndpointBuilder.Poster((string)m.Id)
        )).ToList();

        // Phân trang cho media
        var pagedMedia = allMedia.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var totalCount = allMedia.Count + allArtists.Count + allPlaylists.Count;
        var totalPages = (int)Math.Ceiling(allMedia.Count / (double)pageSize);

        var result = new SearchResultDto(pagedMedia, allArtists, allPlaylists, trending, totalCount);

        return new SearchResponseDto(result, page, pageSize, allMedia.Count, totalPages);
    }
}
