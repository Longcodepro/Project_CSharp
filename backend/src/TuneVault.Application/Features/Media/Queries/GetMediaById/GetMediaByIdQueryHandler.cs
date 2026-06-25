using MediatR;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Media.Queries.GetMediaById;

/// <summary>
/// Handler xử lý <see cref="GetMediaByIdQuery"/>.
/// Luồng: lấy MediaItem từ DB → lấy danh sách nghệ sĩ → map sang DTO.
/// Trả về null (soft not-found) thay vì ném exception.
/// </summary>
public sealed class GetMediaByIdQueryHandler : IRequestHandler<GetMediaByIdQuery, MediaItemDto?>
{
    private readonly IMediaRepository _mediaRepository;

    /// <summary>
    /// Khởi tạo Handler với <see cref="IMediaRepository"/> được inject qua DI.
    /// </summary>
    /// <param name="mediaRepository">Repository thao tác dữ liệu MediaItem.</param>
    public GetMediaByIdQueryHandler(IMediaRepository mediaRepository)
    {
        _mediaRepository = mediaRepository;
    }

    /// <summary>
    /// Xử lý luồng lấy thông tin bài hát.
    /// </summary>
    /// <param name="request">Query chứa MediaId cần tìm.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>DTO thông tin bài hát hoặc <c>null</c> nếu không tồn tại.</returns>
    public async Task<MediaItemDto?> Handle(GetMediaByIdQuery request, CancellationToken ct)
    {
        // Endpoint chi tiết media là public, nên không trả media private hoặc đang bị khóa.
        var mediaItem = await _mediaRepository.GetPublicByIdAsync(request.MediaId, ct);

        // Step 2: Soft not-found — trả null thay vì throw exception
        if (mediaItem is null)
            return null;

        // Step 3: Lấy danh sách nghệ sĩ liên quan (MainArtist + FeaturedArtist)
        var artists = await _mediaRepository.GetArtistsByMediaIdAsync(request.MediaId, ct);

        // Step 4: Map Entity → DTO (không expose file URL trong endpoint này)
        return new MediaItemDto(
            Id:             mediaItem.Id,
            OwnerId:        mediaItem.OwnerId,
            Title:          mediaItem.Title,
            Description:    mediaItem.Description,
            Genre:          mediaItem.Genre,
            Type:           mediaItem.Type.ToString(),
            AudioUrl:       mediaItem.Type != Domain.Enums.MediaType.Video ? MediaEndpointBuilder.AudioStream(mediaItem.Id) : null,
            VideoUrl:       mediaItem.Type == Domain.Enums.MediaType.Video ? MediaEndpointBuilder.VideoStream(mediaItem.Id) : null,
            CoverImageUrl:  string.IsNullOrWhiteSpace(mediaItem.CoverImageUrl) ? null : MediaEndpointBuilder.Poster(mediaItem.Id),
            CanvasUrl:      mediaItem.CanvasUrl,
            DurationSeconds: mediaItem.Duration.TotalSeconds,
            AccessLevel:    mediaItem.AccessLevel.ToString(),
            IsPublic:       mediaItem.IsPublic,
            IsActive:       mediaItem.IsActive,
            IsValid:        mediaItem.IsValid,
            FavoriteCount:  mediaItem.FavoriteCount,
            ViewCount:      mediaItem.ViewCount,
            UploadedAt:     mediaItem.UploadedAt,
            ReleaseDate:    mediaItem.ReleaseDate,
            Artists:        artists.Select(a => new MediaArtistDto(a.ArtistId, a.Role, a.ArtistName))
        );
    }
}
