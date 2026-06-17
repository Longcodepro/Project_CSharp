using MediatR;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Favorite.Queries.GetFavorites;

public sealed class GetFavoritesQueryHandler : IRequestHandler<GetFavoritesQuery, List<MediaItemDto>>
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IMediaRepository _mediaRepository;

    public GetFavoritesQueryHandler(IFavoriteRepository favoriteRepository, IMediaRepository mediaRepository)
    {
        _favoriteRepository = favoriteRepository;
        _mediaRepository = mediaRepository;
    }

    public async Task<List<MediaItemDto>> Handle(GetFavoritesQuery request, CancellationToken cancellationToken)
    {
        var favoriteItems = await _favoriteRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        var result = new List<MediaItemDto>();

        foreach (var favorite in favoriteItems)
        {
            var mediaItem = await _mediaRepository.GetByIdAsync(favorite.MediaItemId, cancellationToken);
            if (mediaItem is null)
                continue;

            var artists = await _mediaRepository.GetArtistsByMediaIdAsync(mediaItem.Id, cancellationToken);
            result.Add(ToDto(mediaItem, artists));
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
