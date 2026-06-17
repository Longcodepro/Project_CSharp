using MediatR;
using TuneVault.Application.DTOs.Search;

namespace TuneVault.Application.Features.Search.Queries.SearchMedia;

/// <summary>
/// Query dùng để tìm kiếm media theo keyword với phân trang.
/// </summary>
/// <param name="Keyword">Từ khóa tìm kiếm.</param>
/// <param name="Page">Số trang kết quả.</param>
/// <param name="PageSize">Số phần tử trên mỗi trang.</param>
public sealed record SearchMediaQuery(string Keyword, int Page = 1, int PageSize = 20) : IRequest<SearchResponseDto>;
