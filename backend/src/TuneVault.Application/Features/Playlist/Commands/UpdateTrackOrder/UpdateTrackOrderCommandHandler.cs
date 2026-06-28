using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Playlist.Commands.UpdateTrackOrder;

/// <summary>
/// Cập nhật thứ tự phát của media trong playlist.
/// </summary>
public sealed class UpdateTrackOrderCommandHandler : IRequestHandler<UpdateTrackOrderCommand, Unit>
{
    private readonly IPlaylistRepository _playlistRepository;

    /// <summary>
    /// Khởi tạo handler cập nhật thứ tự track.
    /// </summary>
    /// <param name="playlistRepository">Repository xử lý truy cập database cho Playlist.</param>
    public UpdateTrackOrderCommandHandler(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    /// <summary>
    /// Đổi vị trí track và sắp xếp lại toàn bộ playlist.
    /// </summary>
    /// <param name="command">Command chứa PlaylistId, MediaItemId và NewTrackOrder.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <exception cref="DomainException">
    /// Ném ra khi Playlist không tồn tại hoặc Track không có trong Playlist.
    /// </exception>
    /// <exception cref="ForbiddenAccessException">Ném ra khi user không phải chủ playlist.</exception>
    public async Task<Unit> Handle(UpdateTrackOrderCommand command, CancellationToken cancellationToken)
    {
        var playlist = await _playlistRepository.GetByIdAsync(command.PlaylistId, cancellationToken)
            ?? throw new DomainException("Không tìm thấy playlist.");

        if (playlist.UserId != command.UserId)
            throw new ForbiddenAccessException("Bạn không có quyền cập nhật playlist này.");

        var tracks = (await _playlistRepository.GetPlaylistTracksAsync(command.PlaylistId, cancellationToken)).ToList();

        var trackToUpdate = tracks.FirstOrDefault(t => t.MediaItemId == command.MediaItemId)
            ?? throw new DomainException("Bài hát không tồn tại trong playlist.");

        if (command.NewTrackOrder < 1 || command.NewTrackOrder > tracks.Count)
            throw new DomainException($"Thứ tự bài hát phải nằm trong khoảng từ 1 đến {tracks.Count}.");

        if (trackToUpdate.TrackOrder == command.NewTrackOrder)
            return Unit.Value;

        var reorderedTracks = tracks
            .Where(track => track.Id != trackToUpdate.Id)
            .OrderBy(track => track.TrackOrder)
            .ToList();

        reorderedTracks.Insert(command.NewTrackOrder - 1, trackToUpdate);

        for (var index = 0; index < reorderedTracks.Count; index++)
        {
            await _playlistRepository.UpdateTrackOrderAsync(
                command.PlaylistId,
                reorderedTracks[index].Id,
                index + 1,
                cancellationToken);
        }

        return Unit.Value;
    }
}
