using MediatR;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Media.Queries.GetUserMedia;

public sealed record GetUserMediaQuery(string UserId) : IRequest<IReadOnlyCollection<MediaItemDto>>;

public sealed class GetUserMediaQueryHandler : IRequestHandler<GetUserMediaQuery, IReadOnlyCollection<MediaItemDto>>
{
    private readonly IMediaRepository _mediaRepository;

    public GetUserMediaQueryHandler(IMediaRepository mediaRepository)
    {
        _mediaRepository = mediaRepository;
    }

    public async Task<IReadOnlyCollection<MediaItemDto>> Handle(GetUserMediaQuery request, CancellationToken ct)
    {
        var items = await _mediaRepository.GetByOwnerAsync(request.UserId, ct);
        var result = new List<MediaItemDto>();

        foreach (var item in items)
        {
            var artists = await _mediaRepository.GetArtistsByMediaIdAsync(item.Id, ct);
            result.Add(new MediaItemDto(
                Id: item.Id,
                OwnerId: item.OwnerId,
                Title: item.Title,
                Description: item.Description,
                Genre: item.Genre,
                Type: item.Type.ToString(),
                AudioUrl: item.Type != MediaType.Video ? MediaEndpointBuilder.AudioStream(item.Id) : null,
                VideoUrl: item.Type == MediaType.Video ? MediaEndpointBuilder.VideoStream(item.Id) : null,
                CoverImageUrl: string.IsNullOrWhiteSpace(item.CoverImageUrl) ? null : MediaEndpointBuilder.Poster(item.Id),
                CanvasUrl: item.CanvasUrl,
                DurationSeconds: item.Duration.TotalSeconds,
                AccessLevel: item.AccessLevel.ToString(),
                IsPublic: item.IsPublic,
                IsActive: item.IsActive,
                IsValid: item.IsValid,
                FavoriteCount: item.FavoriteCount,
                ViewCount: item.ViewCount,
                UploadedAt: item.UploadedAt,
                ReleaseDate: item.ReleaseDate,
                Artists: artists.Select(a => new MediaArtistDto(a.ArtistId, a.Role))
            ));
        }

        return result;
    }
}
