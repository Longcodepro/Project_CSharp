using MediatR;
using TuneVault.Application.DTOs.Search;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Search.Queries.GetTrendingMedia;

/// <summary>
/// Handler xử lý truy vấn lấy media thịnh hành từ repository tìm kiếm.
/// Handler chỉ map dữ liệu sang DTO, còn truy vấn Dapper nằm ở Infrastructure.
/// </summary>
public sealed class GetTrendingMediaQueryHandler
    : IRequestHandler<GetTrendingMediaQuery, IReadOnlyCollection<SearchMediaResultDto>>
{
    private readonly ISearchRepository _searchRepository;

    /// <summary>
    /// Khởi tạo handler với repository tìm kiếm.
    /// </summary>
    /// <param name="searchRepository">Repository đọc dữ liệu search/discovery.</param>
    public GetTrendingMediaQueryHandler(ISearchRepository searchRepository)
    {
        _searchRepository = searchRepository;
    }

    /// <summary>
    /// Lấy media thịnh hành và map sang DTO trả về cho API.
    /// </summary>
    /// <param name="request">Query chứa số lượng media cần lấy.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách media thịnh hành đã được map sang DTO.</returns>
    public async Task<IReadOnlyCollection<SearchMediaResultDto>> Handle(
        GetTrendingMediaQuery request,
        CancellationToken cancellationToken)
    {
        var trendingResults = await _searchRepository.GetTrendingAsync(request.Top, cancellationToken);

        return trendingResults.Cast<dynamic>().Select(m => new SearchMediaResultDto(
            (string)m.Id,
            (string)m.Title,
            (string?)m.ArtistName,
            (string?)m.Genre,
            (int)m.DurationSeconds,
            (int)m.ViewCount,
            string.IsNullOrWhiteSpace((string?)m.CoverImageUrl) ? null : MediaEndpointBuilder.Poster((string)m.Id)
        )).ToList();
    }
}
