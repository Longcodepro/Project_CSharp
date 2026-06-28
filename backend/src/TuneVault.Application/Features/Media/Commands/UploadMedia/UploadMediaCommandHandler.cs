using MediatR;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;
using TuneVault.Domain.ValueObjects;

namespace TuneVault.Application.Features.Media.Commands.UploadMedia;

/// <summary>
/// Handler xử lý <see cref="UploadMediaCommand"/>.
/// Luồng: 
///   1. Kiểm tra CurrentUserId có được xác thực không
///   2. Kiểm tra CurrentUserId == OwnerId (không cho upload cho người khác)
///   3. Verify owner tồn tại và đang hoạt động
///   4. Tạo MediaItem entity
///   5. Persist và trả DTO
/// </summary>
public sealed class UploadMediaCommandHandler : IRequestHandler<UploadMediaCommand, MediaItemDto>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserContext _currentUserContext;

    /// <summary>
    /// Khởi tạo Handler với các dependency repository được inject qua DI.
    /// </summary>
    /// <param name="mediaRepository">Repository thao tác dữ liệu MediaItem.</param>
    /// <param name="userRepository">Repository thao tác dữ liệu User — dùng để validate owner.</param>
    /// <param name="currentUserContext">Service lấy thông tin CurrentUserId từ JWT.</param>
    public UploadMediaCommandHandler(
        IMediaRepository mediaRepository,
        IUserRepository userRepository,
        ICurrentUserContext currentUserContext)
    {
        _mediaRepository = mediaRepository;
        _userRepository = userRepository;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// Xử lý luồng upload bài hát mới.
    /// </summary>
    /// <param name="request">Command chứa MediaId và DTO thông tin bài hát.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>DTO thông tin bài hát vừa được upload.</returns>
    /// <exception cref="UnauthorizedAccessException">Ném ra nếu chưa xác thực.</exception>
    /// <exception cref="ForbiddenAccessException">Ném ra nếu CurrentUserId != OwnerId.</exception>
    /// <exception cref="DomainException">Ném ra nếu owner không tồn tại, không hoạt động hoặc ca sĩ phụ không hợp lệ.</exception>
    public async Task<MediaItemDto> Handle(UploadMediaCommand request, CancellationToken ct)
    {
        var dto = request.Request;

        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new UnauthorizedAccessException("Chưa xác thực. Vui lòng đăng nhập trước khi upload bài hát.");

        if (!currentUserId.Equals(dto.OwnerId, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenAccessException(
                $"Bạn không có quyền upload bài hát cho người dùng '{dto.OwnerId}'. " +
                "Bạn chỉ có thể upload bài hát cho chính mình.");

        var owner = await _userRepository.GetByIdAsync(dto.OwnerId, ct)
            ?? throw new DomainException($"Người dùng với Id '{dto.OwnerId}' không tồn tại.");

        if (!owner.IsActive)
            throw new DomainException("Tài khoản tải media hiện không còn hoạt động.");

        if (!Enum.TryParse<MediaType>(dto.Type, ignoreCase: true, out var mediaType)
            || !Enum.IsDefined(typeof(MediaType), mediaType))
            throw new DomainException($"Loại media '{dto.Type}' không hợp lệ. Các loại hợp lệ: Audio, Video, Song.");

        var rawUrl = mediaType == MediaType.Video
            ? dto.VideoUrl
            : dto.AudioUrl;

        if (string.IsNullOrWhiteSpace(rawUrl))
        {
            throw new DomainException(mediaType == MediaType.Video
                ? "Phải cung cấp file video."
                : "Phải cung cấp file audio.");
        }

        var mediaUrl = new MediaUrl(rawUrl);
        var mediaItem = new MediaItem(request.MediaId, dto.OwnerId, dto.Title, mediaType, mediaUrl);

        if (!string.IsNullOrWhiteSpace(dto.Description) || dto.Genre is not null)
            mediaItem.UpdateDetails(dto.Title, dto.Description, dto.Genre);

        if (!string.IsNullOrWhiteSpace(dto.CoverImageUrl))
            mediaItem.SetCoverImage(dto.CoverImageUrl);

        if (!string.IsNullOrWhiteSpace(dto.CanvasUrl))
            mediaItem.SetCanvas(dto.CanvasUrl);

        if (dto.DurationSeconds is > 0)
            mediaItem.SetDuration(dto.DurationSeconds.Value / 60, dto.DurationSeconds.Value % 60);

        await _mediaRepository.AddAsync(mediaItem, ct);

        return MapToDto(mediaItem, owner.DisplayName);
    }

    /// <summary>
    /// Map <see cref="MediaItem"/> entity sang <see cref="MediaItemDto"/>.
    /// </summary>
    private static MediaItemDto MapToDto(MediaItem mediaItem, string? ownerName)
    {
        return new MediaItemDto(
            Id:             mediaItem.Id,
            OwnerId:        mediaItem.OwnerId,
            Title:          mediaItem.Title,
            Description:    mediaItem.Description,
            Genre:          mediaItem.Genre,
            Type:           mediaItem.Type.ToString(),
            AudioUrl:       mediaItem.Type != MediaType.Video ? MediaEndpointBuilder.AudioStream(mediaItem.Id) : null,
            VideoUrl:       mediaItem.Type == MediaType.Video ? MediaEndpointBuilder.VideoStream(mediaItem.Id) : null,
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
