using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.DTOs.Search;
using TuneVault.Domain.Interfaces;

namespace TuneVault.API.Controllers;

/// <summary>
/// CONTROLLER - SEARCH & DISCOVERY FEATURE (Web API Layer)
/// ========================================================
/// Mục đích: Xử lý HTTP requests/responses cho tìm kiếm và khám phá content.
/// 
/// Luồng xử lý Request:
/// 1. Client gửi HTTP request với keyword
/// 2. Controller nhận request
/// 3. ISearchRepository được DI inject
/// 4. Repository chạy 4 SQL queries song song:
///    - SearchMediaAsync(keyword): Tìm bài hát/podcast
///    - SearchArtistsAsync(keyword): Tìm nghệ sĩ
///    - SearchPlaylistsAsync(keyword): Tìm playlist
///    - GetTrendingAsync(top): Lấy bài nghe nhiều
/// 5. Map dynamic -> DTOs, phân trang media
/// 6. Controller -> HTTP Response
/// 
/// Endpoints:
/// - GET /api/Search?keyword=love&page=1&pageSize=10
/// </summary>

public sealed class SearchController : BaseApiController
{
    private readonly ISearchRepository _searchRepository;

    public SearchController(ISearchRepository searchRepository)
    {
        _searchRepository = searchRepository;
    }

    /// <summary>
    /// Thực hiện tìm kiếm toàn bộ theo keyword với phân trang.
    /// </summary>
    /// <param name="keyword">Từ khóa tìm kiếm.</param>
    /// <param name="page">Số trang (mặc định 1).</param>
    /// <param name="pageSize">Số kết quả mỗi trang (mặc định 10).</param>
    /// <returns>SearchResultDto chứa media, artists, playlists và trending.</returns>
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest("Keyword không được để trống");

        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        var mediaResults = await _searchRepository.SearchMediaAsync(keyword);
        var artistResults = await _searchRepository.SearchArtistsAsync(keyword);
        var playlistResults = await _searchRepository.SearchPlaylistsAsync(keyword);
        var trendingResults = await _searchRepository.GetTrendingAsync(10);

        var allMedia = mediaResults.Cast<dynamic>().Select(m => new SearchMediaResultDto(
            (string)m.Id,
            (string)m.Title,
            (string?)m.ArtistName,
            (string?)m.Genre,
            (int)m.DurationSeconds,
            (int)m.ViewCount,
            (string?)m.CoverImageUrl
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
            (string?)m.CoverImageUrl
        )).ToList();

        // Phân trang cho media
        var pagedMedia = allMedia.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var totalCount = allMedia.Count + allArtists.Count + allPlaylists.Count;

        var result = new SearchResultDto(pagedMedia, allArtists, allPlaylists, trending, totalCount);

        return Ok(new
        {
            data = result,
            page,
            pageSize,
            totalMedia = allMedia.Count,
            totalPages = (int)Math.Ceiling(allMedia.Count / (double)pageSize)
        });
    }
}