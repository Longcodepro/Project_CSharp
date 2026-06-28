using MediatR;
using TuneVault.Domain.Interfaces;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Enums;

namespace TuneVault.Application.Features.History.Queries.GetRecentHistory;

/// <summary>
/// Lấy lịch sử phát gần đây của người dùng.
/// </summary>
public sealed class GetRecentHistoryQueryHandler : IRequestHandler<GetRecentHistoryQuery, List<MediaItemDto>>
{
    private readonly IPlayHistoryRepository _playHistoryRepository;
    private readonly IMediaRepository _mediaRepository;

    public GetRecentHistoryQueryHandler(IPlayHistoryRepository playHistoryRepository, IMediaRepository mediaRepository)
    {
        _playHistoryRepository = playHistoryRepository;
        _mediaRepository = mediaRepository;
    }

    public async Task<List<MediaItemDto>> Handle(GetRecentHistoryQuery request, CancellationToken cancellationToken)
    {
        var playHistoryItems = await _playHistoryRepository.GetRecentByUserIdAsync(request.UserId, ct: cancellationToken);

        var mediaItemDtos = new List<MediaItemDto>();

        foreach (var historyItem in playHistoryItems)
        {
            var mediaItem = await _mediaRepository.GetByIdAsync(historyItem.MediaItemId, cancellationToken);

            if (mediaItem != null)
            {
                var ownerName = await _mediaRepository.GetOwnerDisplayNameAsync(mediaItem.Id, cancellationToken);
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
                    IsPublic: mediaItem.IsPublic,
                    IsActive: mediaItem.IsActive,
                    FavoriteCount: mediaItem.FavoriteCount,
                    ViewCount: mediaItem.ViewCount,
                    UploadedAt: mediaItem.UploadedAt,
                    ReleaseDate: mediaItem.ReleaseDate,
                    OwnerName: ownerName
                );
                mediaItemDtos.Add(mediaItemDto);
            }
        }

        return mediaItemDtos;
    }
}
