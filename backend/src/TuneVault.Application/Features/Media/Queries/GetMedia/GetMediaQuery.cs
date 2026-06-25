using MediatR;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Media.Queries.GetMedia;

/// <summary>
/// Query lấy danh sách media công khai cho người xem bên ngoài.
/// </summary>
public sealed record GetMediaQuery(int Page, int PageSize) : IRequest<IReadOnlyCollection<MediaPublicDto>>;

/// <summary>
/// Handler lấy danh sách media công khai theo phân trang.
/// </summary>
public sealed class GetMediaQueryHandler : IRequestHandler<GetMediaQuery, IReadOnlyCollection<MediaPublicDto>>
{
    private readonly IMediaRepository _mediaRepository;

    public GetMediaQueryHandler(IMediaRepository mediaRepository)
    {
        _mediaRepository = mediaRepository;
    }

    public async Task<IReadOnlyCollection<MediaPublicDto>> Handle(GetMediaQuery request, CancellationToken ct)
    {
        var items = await _mediaRepository.GetPagedAsync(request.Page, request.PageSize, ct);
        return await MapAsync(items, ct);
    }

    private async Task<IReadOnlyCollection<MediaPublicDto>> MapAsync(IReadOnlyCollection<MediaItem> items, CancellationToken ct)
    {
        var result = new List<MediaPublicDto>(items.Count);
        foreach (var item in items)
        {
            var artists = await _mediaRepository.GetArtistsByMediaIdAsync(item.Id, ct);
            result.Add(MediaDtoMapper.ToPublicDto(item, artists));
        }

        return result;
    }
}
