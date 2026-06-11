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
/// 5. Map dynamic -> DTOs
/// 6. Controller -> HTTP Response (SearchResultDto)
/// 
/// Endpoint:
/// - GET /api/search?keyword=love
///   → Search(keyword): Tìm kiếm toàn bộ
///   → Return: SearchResultDto {
///       Media: SearchMediaResultDto[],
///       Artists: SearchArtistResultDto[],
///       Playlists: SearchPlaylistResultDto[],
///       TrendingMedia: SearchMediaResultDto[],
///       TotalCount: int
///     }
/// </summary>

public sealed class SearchController : BaseApiController
{
    private readonly ISearchRepository _searchRepository;

    public SearchController(ISearchRepository searchRepository)
    {
        _searchRepository = searchRepository;
    }

    /// <summary>
    /// Thực hiện tìm kiếm toàn bộ theo keyword.
    /// </summary>
    /// <param name="keyword">Từ khóa tìm kiếm.</param>
    /// <returns>SearchResultDto chứa media, artists, playlists và trending.</returns>
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest("Keyword không được để trống");

        var mediaResults = await _searchRepository.SearchMediaAsync(keyword);
        var artistResults = await _searchRepository.SearchArtistsAsync(keyword);
        var playlistResults = await _searchRepository.SearchPlaylistsAsync(keyword);
        var trendingResults = await _searchRepository.GetTrendingAsync(10);

        var media = mediaResults.Cast<dynamic>().Select(m => new SearchMediaResultDto(
            (string)m.Id,
            (string)m.Title,
            (string?)m.ArtistName,
            (string?)m.Genre,
            (int)m.DurationSeconds,
            (int)m.ViewCount,
            (string?)m.CoverImageUrl
        )).ToList();

        var artists = artistResults.Cast<dynamic>().Select(a => new SearchArtistResultDto(
            (string)a.Id,
            (string)a.UserName,
            (string)a.DisplayName,
            (string?)a.AvatarUrl,
            (int)a.TotalFollowers
        )).ToList();

        var playlists = playlistResults.Cast<dynamic>().Select(p => new SearchPlaylistResultDto(
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

        var totalCount = media.Count + artists.Count + playlists.Count;

        var result = new SearchResultDto(media, artists, playlists, trending, totalCount);

        return Ok(result);
    }
}
