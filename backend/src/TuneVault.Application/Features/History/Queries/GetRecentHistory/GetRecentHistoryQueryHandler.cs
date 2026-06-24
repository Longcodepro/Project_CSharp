using MediatR;
using TuneVault.Domain.Interfaces;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Enums;

namespace TuneVault.Application.Features.History.Queries.GetRecentHistory;

/// <summary>
/// Handler for retrieving the user's recent play history.
/// </summary>
public sealed class GetRecentHistoryQueryHandler : IRequestHandler<GetRecentHistoryQuery, List<MediaItemDto>>
{
    private readonly IPlayHistoryRepository _playHistoryRepository;
    private readonly IMediaRepository _mediaRepository; // Assuming we need this to fetch MediaItem details

    public GetRecentHistoryQueryHandler(IPlayHistoryRepository playHistoryRepository, IMediaRepository mediaRepository)
    {
        _playHistoryRepository = playHistoryRepository;
        _mediaRepository = mediaRepository;
    }

    public async Task<List<MediaItemDto>> Handle(GetRecentHistoryQuery request, CancellationToken cancellationToken)
    {
        // Fetch recent play history items
        var playHistoryItems = await _playHistoryRepository.GetRecentByUserIdAsync(request.UserId, ct: cancellationToken);

        var mediaItemDtos = new List<MediaItemDto>();

        foreach (var historyItem in playHistoryItems)
        {
            // Fetch media item details for each history item
            var mediaItem = await _mediaRepository.GetByIdAsync(historyItem.MediaItemId, cancellationToken);

            if (mediaItem != null)
            {
                // Map PlayHistory and MediaItem to MediaItemDto
                // This mapping needs to be carefully implemented based on the DTO structure and available data.
                var artists = await _mediaRepository.GetArtistsByMediaIdAsync(mediaItem.Id, cancellationToken);
                var mediaItemDto = new MediaItemDto(
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
                mediaItemDtos.Add(mediaItemDto);
            }
        }

        return mediaItemDtos;
    }
}
