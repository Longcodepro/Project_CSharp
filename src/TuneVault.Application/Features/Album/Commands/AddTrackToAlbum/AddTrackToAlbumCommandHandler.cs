using MediatR;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Album.Commands.AddTrackToAlbum;

/// <summary>
/// Handler thêm media vào album và chèn track mới lên đầu danh sách.
/// </summary>
public sealed class AddTrackToAlbumCommandHandler : IRequestHandler<AddTrackToAlbumCommand>
{
    private readonly IAlbumRepository _albumRepository;
    private readonly IMediaRepository _mediaRepository;

    /// <summary>
    /// Khởi tạo handler thêm track vào album.
    /// </summary>
    public AddTrackToAlbumCommandHandler(IAlbumRepository albumRepository, IMediaRepository mediaRepository)
    {
        _albumRepository = albumRepository ?? throw new ArgumentNullException(nameof(albumRepository));
        _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
    }

    /// <summary>
    /// Thêm media vào album nếu owner hợp lệ và media thỏa rule album.
    /// </summary>
    public async Task Handle(AddTrackToAlbumCommand request, CancellationToken cancellationToken)
    {
        var album = await _albumRepository.GetByIdAsync(request.AlbumId, cancellationToken)
            ?? throw new DomainException("Không tìm thấy album.");

        if (!string.Equals(album.ArtistId, request.CurrentUserId, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenAccessException("Bạn không có quyền thêm bài hát vào album này.");

        var media = await _mediaRepository.GetByIdAsync(request.Request.MediaItemId, cancellationToken)
            ?? throw new DomainException("Không tìm thấy media cần thêm vào album.");

        if (!media.IsActive || media.IsValid)
            throw new DomainException("Media này hiện không đủ điều kiện để thêm vào album.");

        if (!string.Equals(media.OwnerId, request.CurrentUserId, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Album chỉ được chứa media do chính bạn sở hữu.");

        if (album.IsPublic && !media.IsPublic)
            throw new DomainException("Album công khai chỉ được chứa media công khai.");

        if (await _albumRepository.TrackExistsAsync(request.AlbumId, request.Request.MediaItemId, cancellationToken))
            throw new DomainException("Media này đã tồn tại trong album.");

        var currentTracks = (await _albumRepository.GetAlbumTracksAsync(request.AlbumId, cancellationToken)).ToList();
        if (currentTracks.Count >= 20)
            throw new DomainException("Album chỉ được chứa tối đa 20 media.");

        if (album.ContentType.HasValue && album.ContentType.Value != media.Type)
            throw new DomainException("Album chỉ được chứa các media cùng một kiểu nội dung.");

        if (!album.ContentType.HasValue)
        {
            album.SetContentType(media.Type);
            await _albumRepository.UpdateAsync(album, cancellationToken);
        }

        if (currentTracks.Count > 0)
        {
            await _albumRepository.ShiftTrackOrdersAsync(request.AlbumId, 1, 1, cancellationToken);
        }

        var trackId = await GenerateNextTrackIdAsync(cancellationToken);
        var track = new AlbumTrack(trackId, request.AlbumId, request.Request.MediaItemId, 1);
        album.AddTrack(track);
        await _albumRepository.AddTrackAsync(track, cancellationToken);
    }

    private async Task<string> GenerateNextTrackIdAsync(CancellationToken cancellationToken)
    {
        const string prefix = "AT";

        var allTracks = await _albumRepository.GetAllTracksAsync(cancellationToken);

        var maxNumber = allTracks
            .Select(t => t.Id)
            .Where(id => id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && id.Length > prefix.Length)
            .Select(id =>
            {
                var numberPart = id[prefix.Length..];
                return int.TryParse(numberPart, out var number) ? number : 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        return $"{prefix}{maxNumber + 1:D3}";
    }
}
