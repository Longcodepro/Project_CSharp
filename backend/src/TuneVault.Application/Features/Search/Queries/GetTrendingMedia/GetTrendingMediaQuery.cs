using MediatR;
using TuneVault.Application.DTOs.Search;

namespace TuneVault.Application.Features.Search.Queries.GetTrendingMedia;

/// <summary>
/// Query lấy danh sách media đang thịnh hành dựa trên lượt nghe/xem.
/// Dùng cho màn hình khám phá mà không cần truyền keyword tìm kiếm.
/// </summary>
/// <param name="Top">Số lượng media muốn lấy, repository sẽ giới hạn trong khoảng an toàn.</param>
public sealed record GetTrendingMediaQuery(int Top = 10) : IRequest<IReadOnlyCollection<SearchMediaResultDto>>;
