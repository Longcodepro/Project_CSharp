using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Features.Search.Queries.SearchMedia;
using TuneVault.Domain.Interfaces;

namespace TuneVault.API.Controllers;

/// <summary>
/// CONTROLLER - SEARCH & DISCOVERY FEATURE (Web API Layer)
/// ========================================================
/// Mục đích: Nhận HTTP request, tạo Query và chuyển cho QueryHandler xử lý.
/// Controller không chứa bất kỳ logic nghiệp vụ nào.
/// 
/// Luồng xử lý:
/// Controller → Query → QueryHandler → Repository → DTO → Response
/// 
/// Endpoints:
/// - GET /api/Search?keyword=love&page=1&pageSize=10  → Tìm kiếm toàn bộ có phân trang
/// - GET /api/Search/genre?genre=Pop                  → Lọc bài hát theo thể loại
/// - GET /api/Search/trending?top=10                  → Lấy top bài nghe nhiều nhất
/// </summary>
public sealed class SearchController : BaseApiController
{
    private readonly SearchMediaQueryHandler _searchHandler;
    private readonly SearchByGenreQueryHandler _genreHandler;
    private readonly GetTrendingQueryHandler _trendingHandler;

    /// <summary>
    /// Khởi tạo Controller với các QueryHandlers được inject qua DI container.
    /// </summary>
    /// <param name="searchRepository">Repository xử lý truy cập database cho Search.</param>
    public SearchController(ISearchRepository searchRepository)
    {
        _searchHandler = new SearchMediaQueryHandler(searchRepository);
        _genreHandler = new SearchByGenreQueryHandler(searchRepository);
        _trendingHandler = new GetTrendingQueryHandler(searchRepository);
    }

    /// <summary>
    /// Thực hiện tìm kiếm toàn bộ theo keyword với phân trang.
    /// </summary>
    /// <param name="keyword">Từ khóa tìm kiếm.</param>
    /// <param name="page">Số trang (mặc định 1).</param>
    /// <param name="pageSize">Số kết quả mỗi trang (mặc định 10, tối đa 50).</param>
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest("Keyword không được để trống");

        var query = new SearchMediaQuery(keyword, page, pageSize);
        var result = await _searchHandler.HandleAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Lọc bài hát theo thể loại (genre).
    /// </summary>
    /// <param name="genre">Thể loại cần lọc, ví dụ: Pop, R&B, Indie.</param>
    [HttpGet("genre")]
    public async Task<IActionResult> SearchByGenre([FromQuery] string genre)
    {
        if (string.IsNullOrWhiteSpace(genre))
            return BadRequest("Genre không được để trống");

        var query = new SearchByGenreQuery(genre);
        var result = await _genreHandler.HandleAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách bài hát nghe nhiều nhất (trending).
    /// </summary>
    /// <param name="top">Số lượng bài muốn lấy (mặc định 10, tối đa 50).</param>
    [HttpGet("trending")]
    public async Task<IActionResult> GetTrending([FromQuery] int top = 10)
    {
        var query = new GetTrendingQuery(top);
        var result = await _trendingHandler.HandleAsync(query);
        return Ok(result);
    }
}