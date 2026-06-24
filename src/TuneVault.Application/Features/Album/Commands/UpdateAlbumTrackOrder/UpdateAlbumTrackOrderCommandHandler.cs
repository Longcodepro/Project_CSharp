using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Album.Commands.UpdateAlbumTrackOrder;

/// <summary>
/// Handler cập nhật thứ tự track trong album.
/// </summary>
public sealed class UpdateAlbumTrackOrderCommandHandler : IRequestHandler<UpdateAlbumTrackOrderCommand>
{
    private readonly IAlbumRepository _albumRepository;

    /// <summary>
    /// Khởi tạo handler cập nhật thứ tự track.
    /// </summary>
    public UpdateAlbumTrackOrderCommandHandler(IAlbumRepository albumRepository)
    {
        _albumRepository = albumRepository ?? throw new ArgumentNullException(nameof(albumRepository));
    }

    /// <summary>
    /// Đưa track tới vị trí mới và đánh lại toàn bộ thứ tự trong album.
    /// </summary>
    public async Task Handle(UpdateAlbumTrackOrderCommand request, CancellationToken cancellationToken)
    {
        var album = await _albumRepository.GetByIdAsync(request.AlbumId, cancellationToken)
            ?? throw new DomainException("Không tìm thấy album.");

        if (!string.Equals(album.ArtistId, request.CurrentUserId, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenAccessException("Bạn không có quyền cập nhật album này.");

        var tracks = (await _albumRepository.GetAlbumTracksAsync(request.AlbumId, cancellationToken)).ToList();
        var trackToUpdate = tracks.FirstOrDefault(t => t.MediaItemId == request.MediaItemId)
            ?? throw new DomainException("Media này không tồn tại trong album.");

        if (request.NewOrder < 1 || request.NewOrder > tracks.Count)
            throw new DomainException($"Thứ tự media phải nằm trong khoảng từ 1 đến {tracks.Count}.");

        if (trackToUpdate.TrackOrder == request.NewOrder)
            return;

        var reordered = tracks
            .Where(t => t.Id != trackToUpdate.Id)
            .OrderBy(t => t.TrackOrder)
            .ToList();

        reordered.Insert(request.NewOrder - 1, trackToUpdate);

        for (var index = 0; index < reordered.Count; index++)
        {
            await _albumRepository.UpdateTrackOrderAsync(request.AlbumId, reordered[index].Id, index + 1, cancellationToken);
        }
    }
}
