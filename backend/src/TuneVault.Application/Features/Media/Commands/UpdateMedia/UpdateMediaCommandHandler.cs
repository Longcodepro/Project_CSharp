using MediatR;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;
using TuneVault.Domain.ValueObjects;

namespace TuneVault.Application.Features.Media.Commands.UpdateMedia;

/// <summary>
/// Handler xử lý <see cref="UpdateMediaCommand"/>.
/// Luồng: lấy Entity → kiểm tra quyền → cập nhật metadata qua method Entity
///         → persist → lấy tên owner → trả về DTO.
/// </summary>
public sealed class UpdateMediaCommandHandler : IRequestHandler<UpdateMediaCommand, MediaItemDto>
{
    private readonly IMediaRepository _mediaRepository;

    /// <summary>
    /// Khởi tạo Handler với <see cref="IMediaRepository"/> được inject qua DI.
    /// </summary>
    /// <param name="mediaRepository">Repository thao tác dữ liệu MediaItem.</param>
    public UpdateMediaCommandHandler(IMediaRepository mediaRepository)
    {
        _mediaRepository = mediaRepository;
    }

    /// <summary>
    /// Xử lý luồng cập nhật bài hát theo thứ tự:
    /// lấy Entity → validate tồn tại + quyền → cập nhật metadata → persist → map DTO.
    /// </summary>
    /// <param name="request">Command chứa MediaId, RequesterId và DTO thông tin mới.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>DTO thông tin bài hát sau khi cập nhật.</returns>
    /// <exception cref="DomainException">Ném ra nếu bài hát không tồn tại, đã xóa, hoặc không có quyền.</exception>
    public async Task<MediaItemDto> Handle(UpdateMediaCommand request, CancellationToken ct)
    {
        var dto = request.Request;

        var mediaItem = await _mediaRepository.GetByIdAsync(request.MediaId, ct)
            ?? throw new DomainException($"Bài hát với Id '{request.MediaId}' không tồn tại hoặc đã bị xóa.");

        if (mediaItem.OwnerId != request.RequesterId)
            throw new ForbiddenAccessException(
                $"Bạn không có quyền chỉnh sửa bài hát này. Chỉ ca sĩ chính (Owner) mới có thể cập nhật bài hát.");

        mediaItem.UpdateDetails(dto.Title, dto.Description, dto.Genre);

        if (!string.IsNullOrWhiteSpace(dto.MediaUrl))
            mediaItem.SetMediaUrl(new MediaUrl(dto.MediaUrl));

        if (!string.IsNullOrWhiteSpace(dto.CoverImageUrl))
            mediaItem.SetCoverImage(dto.CoverImageUrl);

        if (!string.IsNullOrWhiteSpace(dto.CanvasUrl))
            mediaItem.SetCanvas(dto.CanvasUrl);

        if (dto.DurationSeconds is > 0)
            mediaItem.SetDuration(dto.DurationSeconds.Value / 60, dto.DurationSeconds.Value % 60);

        mediaItem.SetVisibility(dto.IsPublic);

        await _mediaRepository.UpdateAsync(mediaItem, ct);

        var ownerName = await _mediaRepository.GetOwnerDisplayNameAsync(request.MediaId, ct);

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
            IsPublic:       mediaItem.IsPublic,
            IsActive:       mediaItem.IsActive,
            FavoriteCount:  mediaItem.FavoriteCount,
            ViewCount:      mediaItem.ViewCount,
            UploadedAt:     mediaItem.UploadedAt,
            ReleaseDate:    mediaItem.ReleaseDate,
            OwnerName:      ownerName
        );
    }
}
