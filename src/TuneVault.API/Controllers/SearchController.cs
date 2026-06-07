using Microsoft.AspNetCore.Mvc;
using TuneVault.Domain.Interfaces;

namespace TuneVault.API.Controllers;

/// <summary>
/// SUMMARY PHẦN TÌM KIẾM & KHÁM PHÁ - API CONTROLLER
/// File này tạo endpoint API cho chức năng Search/Discovery.
/// 
/// Nhiệm vụ được cover:
/// - GET /api/search?keyword=...              -> search tổng hợp.
/// - GET /api/search/media?keyword=...        -> tìm bài hát / podcast.
/// - GET /api/search/artists?keyword=...      -> tìm nghệ sĩ.
/// - GET /api/search/albums?keyword=...       -> tìm album.
/// - GET /api/search/playlists?keyword=...    -> tìm playlist.
/// - GET /api/search/genre?genre=...          -> lọc theo genre.
/// - GET /api/search/trending?top=10          -> bài nghe nhiều nhất.
/// 
/// Controller chỉ nhận request, validate keyword/genre, rồi gọi Repository.
/// SQL nằm bên SearchRepository, không viết SQL ở Controller.
/// </summary>
public sealed class SearchController : BaseApiController
{
    private readonly ISearchRepository _searchRepository;

    public SearchController(ISearchRepository searchRepository)
    {
        _searchRepository = searchRepository;
    }

    /// <summary>
    /// Search tổng hợp: trả về media, artists, albums, playlists cùng lúc.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SearchAll([FromQuery] string keyword, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(new { message = "keyword không được để trống" });

        var media = await _searchRepository.SearchMediaAsync(keyword, cancellationToken);
        var artists = await _searchRepository.SearchArtistsAsync(keyword, cancellationToken);
        var albums = await _searchRepository.SearchAlbumsAsync(keyword, cancellationToken);
        var playlists = await _searchRepository.SearchPlaylistsAsync(keyword, cancellationToken);

        return Ok(new
        {
            keyword,
            media,
            artists,
            albums,
            playlists
        });
    }

    /// <summary>
    /// Tìm bài hát / podcast.
    /// </summary>
    [HttpGet("media")]
    public async Task<IActionResult> SearchMedia([FromQuery] string keyword, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(new { message = "keyword không được để trống" });

        var result = await _searchRepository.SearchMediaAsync(keyword, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tìm nghệ sĩ.
    /// </summary>
    [HttpGet("artists")]
    public async Task<IActionResult> SearchArtists([FromQuery] string keyword, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(new { message = "keyword không được để trống" });

        var result = await _searchRepository.SearchArtistsAsync(keyword, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tìm album.
    /// </summary>
    [HttpGet("albums")]
    public async Task<IActionResult> SearchAlbums([FromQuery] string keyword, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(new { message = "keyword không được để trống" });

        var result = await _searchRepository.SearchAlbumsAsync(keyword, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tìm playlist.
    /// </summary>
    [HttpGet("playlists")]
    public async Task<IActionResult> SearchPlaylists([FromQuery] string keyword, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(new { message = "keyword không được để trống" });

        var result = await _searchRepository.SearchPlaylistsAsync(keyword, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lọc bài hát / podcast theo genre.
    /// </summary>
    [HttpGet("genre")]
    public async Task<IActionResult> SearchByGenre([FromQuery] string genre, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(genre))
            return BadRequest(new { message = "genre không được để trống" });

        var result = await _searchRepository.SearchByGenreAsync(genre, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách bài nghe nhiều nhất.
    /// </summary>
    [HttpGet("trending")]
    public async Task<IActionResult> GetTrending([FromQuery] int top = 10, CancellationToken cancellationToken = default)
    {
        var result = await _searchRepository.GetTrendingAsync(top, cancellationToken);
        return Ok(result);
    }
}
