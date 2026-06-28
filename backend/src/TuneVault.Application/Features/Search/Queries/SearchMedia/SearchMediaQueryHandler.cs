using MediatR;
using TuneVault.Application.DTOs.Search;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Search.Queries.SearchMedia;

/// <summary>
/// Xử lý tìm kiếm nội dung theo từ khóa.
/// </summary>
public sealed class SearchMediaQueryHandler : IRequestHandler<SearchMediaQuery, SearchResponseDto>
{
    private readonly ISearchRepository _searchRepository;

    /// <summary>
    /// Khởi tạo handler tìm kiếm.
    /// </summary>
    /// <param name="searchRepository">Repository xử lý truy cập database cho Search.</param>
    public SearchMediaQueryHandler(ISearchRepository searchRepository)
    {
        _searchRepository = searchRepository;
    }

    /// <summary>
    /// Tìm kiếm media, user, playlist và trả về dữ liệu phân trang.
    /// </summary>
    /// <param name="query">Query chứa Keyword, Page và PageSize.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Object chứa data kết quả, thông tin phân trang.</returns>
    public async Task<SearchResponseDto> Handle(SearchMediaQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = (query.PageSize < 1 || query.PageSize > 50) ? 10 : query.PageSize;

        var mediaResults = await _searchRepository.SearchMediaAsync(query.Keyword, cancellationToken);
        var artistResults = await _searchRepository.SearchArtistsAsync(query.Keyword, cancellationToken);
        var playlistResults = await _searchRepository.SearchPlaylistsAsync(query.Keyword, cancellationToken);
        var trendingResults = await _searchRepository.GetTrendingAsync(10, cancellationToken);

        var allMedia = mediaResults.Cast<dynamic>().Select(m => new SearchMediaResultDto(
            (string)m.Id,
            (string)m.Title,
            (string?)m.ArtistName,
            (string?)m.Genre,
            (int)m.DurationSeconds,
            (int)m.ViewCount,
            string.IsNullOrWhiteSpace((string?)m.CoverImageUrl) ? null : MediaEndpointBuilder.Poster((string)m.Id)
        )).ToList();

        var allArtists = artistResults.Cast<dynamic>().Select(a => new SearchArtistResultDto(
            (string)a.Id,
            (string)a.UserName,
            (string)a.DisplayName,
            (string?)a.AvatarUrl,
            (int)a.TotalFollowers
        )).ToList();

        var allPlaylists = playlistResults.Cast<dynamic>().Select(p => new SearchPlaylistResultDto(
            (string)p.Id,
            (string)p.Title,
            (string?)p.CoverImageUrl,
            (string)p.OwnerName,
            (int)p.TrackCount,
            (DateTime)p.CreatedAt
        )).ToList();

        var trending = trendingResults.Cast<dynamic>().Select(m => new SearchMediaResultDto(
            (string)m.Id,
            (string)m.Title,
            (string?)m.ArtistName,
            (string?)m.Genre,
            (int)m.DurationSeconds,
            (int)m.ViewCount,
            string.IsNullOrWhiteSpace((string?)m.CoverImageUrl) ? null : MediaEndpointBuilder.Poster((string)m.Id)
        )).ToList();

        var pagedMedia = allMedia.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var totalCount = allMedia.Count + allArtists.Count + allPlaylists.Count;
        var totalPages = (int)Math.Ceiling(allMedia.Count / (double)pageSize);

        var result = new SearchResultDto(pagedMedia, allArtists, allPlaylists, trending, totalCount);

        return new SearchResponseDto(result, page, pageSize, allMedia.Count, totalPages);
    }
}
