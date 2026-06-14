using MediatR;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Media.Commands.UpdateMedia;

/// <summary>
/// Handler xử lý <see cref="UpdateMediaCommand"/>.
/// Luồng: lấy Entity → kiểm tra quyền → cập nhật metadata qua method Entity
///         → persist → lấy lại danh sách nghệ sĩ → trả về DTO.
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

        // Step 1: Lấy MediaItem Entity từ database
        var mediaItem = await _mediaRepository.GetByIdAsync(request.MediaId, ct)
            ?? throw new DomainException($"Bài hát với Id '{request.MediaId}' không tồn tại hoặc đã bị xóa.");

        // Step 2: Kiểm tra quyền — chỉ OwnerId (ca sĩ chính) mới được cập nhật
        if (mediaItem.OwnerId != request.RequesterId)
            throw new ForbiddenAccessException(
                $"Bạn không có quyền chỉnh sửa bài hát này. Chỉ ca sĩ chính (Owner) mới có thể cập nhật bài hát.");

        // Step 3: Cập nhật metadata qua method nghiệp vụ của Entity (Entity tự validate)
        mediaItem.UpdateDetails(dto.Title, dto.Description, dto.Genre);

        // Step 4: Cập nhật ảnh bìa nếu được cung cấp
        if (!string.IsNullOrWhiteSpace(dto.CoverImageUrl))
            mediaItem.SetCoverImage(dto.CoverImageUrl);

        // Step 5: Cập nhật Canvas nếu được cung cấp
        if (!string.IsNullOrWhiteSpace(dto.CanvasUrl))
            mediaItem.SetCanvas(dto.CanvasUrl);

        // Step 6: Cập nhật chính sách truy cập (giữ nguyên trailer nếu đã có)
        mediaItem.UpdateAccessPolicy((AccessLevel)dto.AccessLevel, 0, 0);

        // Step 7: Persist Entity vào database
        await _mediaRepository.UpdateAsync(mediaItem, ct);

        // Step 8: Lấy danh sách nghệ sĩ để trả về DTO đầy đủ
        var artists = await _mediaRepository.GetArtistsByMediaIdAsync(request.MediaId, ct);

        // Step 9: Map Entity + artists → DTO
        return new MediaItemDto(
            Id:             mediaItem.Id,
            OwnerId:        mediaItem.OwnerId,
            Title:          mediaItem.Title,
            Description:    mediaItem.Description,
            Genre:          mediaItem.Genre,
            Type:           mediaItem.Type.ToString(),
            AudioUrl:       mediaItem.Type != Domain.Enums.MediaType.Video ? mediaItem.Url.Value : null,
            VideoUrl:       mediaItem.Type == Domain.Enums.MediaType.Video ? mediaItem.Url.Value : null,
            CoverImageUrl:  mediaItem.CoverImageUrl,
            CanvasUrl:      mediaItem.CanvasUrl,
            DurationSeconds: mediaItem.Duration.TotalSeconds,
            AccessLevel:    mediaItem.AccessLevel.ToString(),
            IsPublic:       mediaItem.IsPublic,
            IsActive:       mediaItem.IsActive,
            FavoriteCount:  mediaItem.FavoriteCount,
            ViewCount:      mediaItem.ViewCount,
            UploadedAt:     mediaItem.UploadedAt,
            ReleaseDate:    mediaItem.ReleaseDate,
            Artists:        artists.Select(a => new MediaArtistDto(a.ArtistId, a.Role))
        );
    }
}