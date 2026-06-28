using TuneVault.Domain.Entities;
using TuneVault.Domain.Enums;

namespace TuneVault.Application.Features.Media.DTOs;

/// <summary>
/// Tập helper map entity media sang các DTO trả về cho từng ngữ cảnh hiển thị.
/// </summary>
public static class MediaDtoMapper
{
    /// <summary>
    /// Map media sang DTO công khai dành cho người xem thông thường.
    /// </summary>
    public static MediaPublicDto ToPublicDto(MediaItem mediaItem, string? ownerName)
    {
        return new MediaPublicDto(
            Id: mediaItem.Id,
            OwnerId: mediaItem.OwnerId,
            OwnerName: ownerName,
            Title: mediaItem.Title,
            Description: mediaItem.Description,
            Genre: mediaItem.Genre,
            Type: mediaItem.Type.ToString(),
            AudioUrl: mediaItem.Type != MediaType.Video ? MediaEndpointBuilder.AudioStream(mediaItem.Id) : null,
            VideoUrl: mediaItem.Type == MediaType.Video ? MediaEndpointBuilder.VideoStream(mediaItem.Id) : null,
            CoverImageUrl: string.IsNullOrWhiteSpace(mediaItem.CoverImageUrl) ? null : MediaEndpointBuilder.Poster(mediaItem.Id),
            FavoriteCount: mediaItem.FavoriteCount,
            ViewCount: mediaItem.ViewCount,
            UploadedAt: mediaItem.UploadedAt,
            ReleaseDate: mediaItem.ReleaseDate
        );
    }

    /// <summary>
    /// Map media sang DTO chi tiết dành cho owner.
    /// </summary>
    public static MediaOwnerDetailDto ToOwnerDetailDto(MediaItem mediaItem, string? ownerName)
    {
        return new MediaOwnerDetailDto(
            Id: mediaItem.Id,
            OwnerId: mediaItem.OwnerId,
            OwnerName: ownerName,
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
            ReleaseDate: mediaItem.ReleaseDate
        );
    }
}
