using MediatR;
using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Common;
using TuneVault.Application.DTOs.Search;
using TuneVault.Application.Features.Search.Queries.GetTrendingMedia;
using TuneVault.Application.Features.Search.Queries.SearchMedia;

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
/// - GET /api/search?keyword=love&page=1&pageSize=10  → Tìm kiếm toàn bộ có phân trang
/// - GET /api/search/trending?top=10                  → Lấy top bài nghe nhiều nhất
/// </summary>
[Route("api/search")]
public sealed class SearchController : BaseApiController
{
    private readonly ISender _mediator;

    /// <summary>
    /// Khởi tạo controller với MediatR sender.
    /// </summary>
    /// <param name="mediator">Sender dùng để gửi query sang Application layer.</param>
    public SearchController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Thực hiện tìm kiếm toàn bộ theo keyword với phân trang.
    /// </summary>
    /// <param name="keyword">Từ khóa tìm kiếm.</param>
    /// <param name="page">Số trang (mặc định 1).</param>
    /// <param name="pageSize">Số kết quả mỗi trang (mặc định 10, tối đa 50).</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SearchResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(ApiResponse<object?>.Fail("Từ khóa tìm kiếm không được để trống."));

        var query = new SearchMediaQuery(keyword, page, pageSize);
        var result = await _mediator.Send(query, ct);
        return Ok(ApiResponse<SearchResponseDto>.Ok(result, "Tìm kiếm thành công."));
    }

    /// <summary>
    /// Lấy danh sách media thịnh hành cho màn hình khám phá.
    /// </summary>
    /// <param name="top">Số lượng media cần lấy, tối đa được giới hạn trong repository.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>ApiResponse chứa danh sách media thịnh hành.</returns>
    [HttpGet("trending")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SearchMediaResultDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrending([FromQuery] int top = 10, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetTrendingMediaQuery(top), ct);

        return Ok(ApiResponse<IReadOnlyCollection<SearchMediaResultDto>>.Ok(result, "Lấy danh sách media thịnh hành thành công."));
    }

}
