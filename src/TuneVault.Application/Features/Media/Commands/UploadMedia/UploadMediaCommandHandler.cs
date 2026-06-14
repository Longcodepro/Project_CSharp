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
///   3. Verify owner tồn tại && là Artist
///   4. Tạo MediaItem entity
///   5. Gán ca sĩ chính & phụ
///   6. Persist và trả DTO
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
    /// <exception cref="DomainException">Ném ra nếu owner không tồn tại, không phải Artist, hoặc ca sĩ phụ không hợp lệ.</exception>
    public async Task<MediaItemDto> Handle(UploadMediaCommand request, CancellationToken ct)
    {
        var dto = request.Request;

        // Step 1: Kiểm tra CurrentUserId có được xác thực không
        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new UnauthorizedAccessException("Chưa xác thực. Vui lòng đăng nhập trước khi upload bài hát.");

        // Step 2: Kiểm tra CurrentUserId == OwnerId (bảo vệ ownership)
        if (!currentUserId.Equals(dto.OwnerId, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenAccessException(
                $"Bạn không có quyền upload bài hát cho người dùng '{dto.OwnerId}'. " +
                "Bạn chỉ có thể upload bài hát cho chính mình.");

        // Step 3: Kiểm tra ca sĩ chính (OwnerId) có tồn tại và là Artist không
        var owner = await _userRepository.GetByIdAsync(dto.OwnerId, ct)
            ?? throw new DomainException($"Người dùng với Id '{dto.OwnerId}' không tồn tại.");

        if (!owner.IsArtist)
            throw new DomainException($"Người dùng '{dto.OwnerId}' không phải là Artist và không có quyền upload bài hát.");

        // Step 4: Parse MediaType từ string — ném lỗi nếu không hợp lệ
        if (!Enum.TryParse<MediaType>(dto.Type, ignoreCase: true, out var mediaType))
            throw new DomainException($"Loại media '{dto.Type}' không hợp lệ. Các loại hợp lệ: Audio, Video, Podcast, Song.");

        // Step 5: Xác định URL file media chính (ưu tiên AudioUrl, fallback VideoUrl)
        var rawUrl = dto.AudioUrl ?? dto.VideoUrl
            ?? throw new DomainException("Phải cung cấp ít nhất một trong AudioUrl hoặc VideoUrl.");

        var mediaUrl = new MediaUrl(rawUrl);
        var accessLevel = (AccessLevel)dto.AccessLevel;

        // Step 6: Tạo MediaItem Entity thông qua constructor — Entity tự validate dữ liệu
        var mediaItem = new MediaItem(request.MediaId, dto.OwnerId, dto.Title, mediaType, mediaUrl, accessLevel);

        // Step 7: Gán các thông tin metadata tùy chọn
        if (!string.IsNullOrWhiteSpace(dto.Description) || dto.Genre is not null)
            mediaItem.UpdateDetails(dto.Title, dto.Description, dto.Genre);

        if (!string.IsNullOrWhiteSpace(dto.CoverImageUrl))
            mediaItem.SetCoverImage(dto.CoverImageUrl);

        if (!string.IsNullOrWhiteSpace(dto.CanvasUrl))
            mediaItem.SetCanvas(dto.CanvasUrl);

        // Step 8: Persist MediaItem vào DB
        await _mediaRepository.AddAsync(mediaItem, ct);

        // Step 9: Xây dựng danh sách quan hệ nghệ sĩ
        var artists = new List<MediaArtist>
        {
            // Ca sĩ chính luôn được thêm đầu tiên với role MainArtist
            new MediaArtist
            {
                MediaItemId = request.MediaId,
                ArtistId    = dto.OwnerId,
                Role        = "MainArtist"
            }
        };

        // Step 10: Validate và thêm ca sĩ phụ (FeaturedArtist)
        var featuredIds = dto.FeaturedArtistIds?.ToList() ?? new List<string>();
        foreach (var featuredId in featuredIds)
        {
            if (featuredId == dto.OwnerId)
                throw new DomainException($"Ca sĩ chính '{dto.OwnerId}' không thể đồng thời là ca sĩ phụ.");

            var featuredArtist = await _userRepository.GetByIdAsync(featuredId, ct)
                ?? throw new DomainException($"Ca sĩ phụ với Id '{featuredId}' không tồn tại.");

            if (!featuredArtist.IsArtist)
                throw new DomainException($"Người dùng '{featuredId}' không phải là Artist.");

            artists.Add(new MediaArtist
            {
                MediaItemId = request.MediaId,
                ArtistId    = featuredId,
                Role        = "FeaturedArtist"
            });
        }

        // Step 11: Persist danh sách nghệ sĩ vào bảng MediaArtists
        await _mediaRepository.AddArtistsAsync(artists, ct);

        // Step 12: Map Entity → DTO và trả về cho Controller
        return MapToDto(mediaItem, artists);
    }

    /// <summary>
    /// Map <see cref="MediaItem"/> entity và danh sách <see cref="MediaArtist"/> sang <see cref="MediaItemDto"/>.
    /// </summary>
    private static MediaItemDto MapToDto(MediaItem mediaItem, IEnumerable<MediaArtist> artists)
    {
        return new MediaItemDto(
            Id:             mediaItem.Id,
            OwnerId:        mediaItem.OwnerId,
            Title:          mediaItem.Title,
            Description:    mediaItem.Description,
            Genre:          mediaItem.Genre,
            Type:           mediaItem.Type.ToString(),
            AudioUrl:       mediaItem.Url.Value.Contains("audio") || !mediaItem.Url.Value.Contains("video")
                                ? mediaItem.Url.Value : null,
            VideoUrl:       mediaItem.Type == MediaType.Video ? mediaItem.Url.Value : null,
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