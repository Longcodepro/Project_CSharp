using MediatR;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Media.Queries.GetMedia;

public sealed record GetMediaQuery(int Page, int PageSize) : IRequest<IReadOnlyCollection<MediaItemDto>>;

public sealed class GetMediaQueryHandler : IRequestHandler<GetMediaQuery, IReadOnlyCollection<MediaItemDto>>
{
    private readonly IMediaRepository _mediaRepository;

    public GetMediaQueryHandler(IMediaRepository mediaRepository)
    {
        _mediaRepository = mediaRepository;
    }

    public async Task<IReadOnlyCollection<MediaItemDto>> Handle(GetMediaQuery request, CancellationToken ct)
    {
        var items = await _mediaRepository.GetPagedAsync(request.Page, request.PageSize, ct);
        return await MapAsync(items, ct);
    }

    private async Task<IReadOnlyCollection<MediaItemDto>> MapAsync(IReadOnlyCollection<MediaItem> items, CancellationToken ct)
    {
        var result = new List<MediaItemDto>();
        foreach (var item in items)
        {
            var artists = await _mediaRepository.GetArtistsByMediaIdAsync(item.Id, ct);
            result.Add(ToDto(item, artists));
        }

        return result;
    }

    private static MediaItemDto ToDto(MediaItem mediaItem, IEnumerable<MediaArtist> artists)
    {
        return new MediaItemDto(
            Id: mediaItem.Id,
            OwnerId: mediaItem.OwnerId,
            Title: mediaItem.Title,
            Description: mediaItem.Description,
            Genre: mediaItem.Genre,
            Type: mediaItem.Type.ToString(),
            AudioUrl: mediaItem.Type != MediaType.Video ? MediaEndpointBuilder.AudioStream(mediaItem.Id) : null,
            VideoUrl: mediaItem.Type == MediaType.Video ? MediaEndpointBuilder.VideoStream(mediaItem.Id) : null,
            CoverImageUrl: string.IsNullOrWhiteSpace(mediaItem.CoverImageUrl) ? null : MediaEndpointBuilder.Poster(mediaItem.Id),
            CanvasUrl: mediaItem.CanvasUrl,
            DurationSeconds: mediaItem.Duration.TotalSeconds,
            AccessLevel: mediaItem.AccessLevel.ToString(),
            IsPublic: mediaItem.IsPublic,
            IsActive: mediaItem.IsActive,
            IsValid: mediaItem.IsValid,
            FavoriteCount: mediaItem.FavoriteCount,
            ViewCount: mediaItem.ViewCount,
            UploadedAt: mediaItem.UploadedAt,
            ReleaseDate: mediaItem.ReleaseDate,
            Artists: artists.Select(a => new MediaArtistDto(a.ArtistId, a.Role))
        );
    }
}
