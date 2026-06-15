using TuneVault.Application.DTOs.Search;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Search.Queries.SearchMedia;

/// <summary>
/// QUERY - LẤY TOP BÀI NGHE NHIỀU NHẤT (Application Layer)
/// =========================================================
/// Mục đích: Đóng gói thông tin cần thiết để lấy danh sách trending.
/// </summary>
/// <param name="Top">Số lượng bài muốn lấy (mặc định 10, tối đa 50).</param>
public sealed record GetTrendingQuery(int Top = 10);

/// <summary>
/// QUERY HANDLER - LẤY TOP BÀI NGHE NHIỀU NHẤT (Application Layer)
/// =================================================================
/// Mục đích: Xử lý logic truy vấn lấy danh sách bài hát trending.
/// 
/// Luồng xử lý:
/// 1. Controller gửi GetTrendingQuery
/// 2. Handler gọi Repository lấy top bài hát theo ViewCount
/// 3. Map dynamic -> SearchMediaResultDto
/// 4. Trả về danh sách cho Controller
/// </summary>
public sealed class GetTrendingQueryHandler
{
    private readonly ISearchRepository _searchRepository;

    /// <summary>
    /// Khởi tạo Handler với Repository được inject qua DI container.
    /// </summary>
    /// <param name="searchRepository">Repository xử lý truy cập database cho Search.</param>
    public GetTrendingQueryHandler(ISearchRepository searchRepository)
    {
        _searchRepository = searchRepository;
    }

    /// <summary>
    /// Thực thi logic lấy danh sách trending từ Query.
    /// </summary>
    /// <param name="query">Query chứa số lượng bài muốn lấy.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách SearchMediaResultDto theo ViewCount giảm dần.</returns>
    public async Task<IReadOnlyCollection<SearchMediaResultDto>> HandleAsync(GetTrendingQuery query, CancellationToken cancellationToken = default)
    {
        var top = Math.Clamp(query.Top, 1, 50);
        var results = await _searchRepository.GetTrendingAsync(top, cancellationToken);

        return results.Cast<dynamic>().Select(m => new SearchMediaResultDto(
            (string)m.Id,
            (string)m.Title,
            (string?)m.ArtistName,
            (string?)m.Genre,
            (int)m.DurationSeconds,
            (int)m.ViewCount,
            (string?)m.CoverImageUrl
        )).ToList();
    }
}
