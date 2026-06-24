using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Album.Commands.RemoveTrackFromAlbum;

/// <summary>
/// Handler xóa track khỏi album và dồn lại thứ tự phát.
/// </summary>
public sealed class RemoveTrackFromAlbumCommandHandler : IRequestHandler<RemoveTrackFromAlbumCommand>
{
    private readonly IAlbumRepository _albumRepository;

    /// <summary>
    /// Khởi tạo handler xóa track khỏi album.
    /// </summary>
    public RemoveTrackFromAlbumCommandHandler(IAlbumRepository albumRepository)
    {
        _albumRepository = albumRepository ?? throw new ArgumentNullException(nameof(albumRepository));
    }

    /// <summary>
    /// Xóa media khỏi album nếu owner hợp lệ.
    /// </summary>
    public async Task Handle(RemoveTrackFromAlbumCommand request, CancellationToken cancellationToken)
    {
        var album = await _albumRepository.GetByIdAsync(request.AlbumId, cancellationToken)
            ?? throw new DomainException("Không tìm thấy album.");

        if (!string.Equals(album.ArtistId, request.CurrentUserId, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenAccessException("Bạn không có quyền xóa media khỏi album này.");

        var tracks = (await _albumRepository.GetAlbumTracksAsync(request.AlbumId, cancellationToken)).ToList();
        var trackToRemove = tracks.FirstOrDefault(t => t.MediaItemId == request.MediaItemId)
            ?? throw new DomainException("Media này không tồn tại trong album.");

        await _albumRepository.RemoveTrackAsync(request.AlbumId, request.MediaItemId, cancellationToken);
        await _albumRepository.ShiftTrackOrdersAsync(request.AlbumId, trackToRemove.TrackOrder + 1, -1, cancellationToken);
    }
}
