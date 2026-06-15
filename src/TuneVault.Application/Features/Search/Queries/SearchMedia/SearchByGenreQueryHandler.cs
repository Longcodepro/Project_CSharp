using TuneVault.Application.DTOs.Search;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Search.Queries.SearchMedia;

/// <summary>
/// QUERY - LỌC BÀI HÁT THEO THỂ LOẠI (Application Layer)
/// =======================================================
/// Mục đích: Đóng gói thông tin cần thiết để lọc bài hát theo genre.
/// </summary>
/// <param name="Genre">Thể loại cần lọc, ví dụ: Pop, R&B, Indie.</param>
public sealed record SearchByGenreQuery(string Genre);

/// <summary>
/// QUERY HANDLER - LỌC BÀI HÁT THEO THỂ LOẠI (Application Layer)
/// ===============================================================
/// Mục đích: Xử lý logic truy vấn lọc bài hát theo genre.
/// 
/// Luồng xử lý:
/// 1. Controller gửi SearchByGenreQuery
/// 2. Handler gọi Repository lấy danh sách bài hát theo genre
/// 3. Map dynamic -> SearchMediaResultDto
/// 4. Trả về danh sách cho Controller
/// </summary>
public sealed class SearchByGenreQueryHandler
{
    private readonly ISearchRepository _searchRepository;

    /// <summary>
    /// Khởi tạo Handler với Repository được inject qua DI container.
    /// </summary>
    /// <param name="searchRepository">Repository xử lý truy cập database cho Search.</param>
    public SearchByGenreQueryHandler(ISearchRepository searchRepository)
    {
        _searchRepository = searchRepository;
    }

    /// <summary>
    /// Thực thi logic lọc bài hát theo genre từ Query.
    /// </summary>
    /// <param name="query">Query chứa Genre cần lọc.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách SearchMediaResultDto theo genre.</returns>
    public async Task<IReadOnlyCollection<SearchMediaResultDto>> HandleAsync(SearchByGenreQuery query, CancellationToken cancellationToken = default)
    {
        var results = await _searchRepository.SearchByGenreAsync(query.Genre, cancellationToken);

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
