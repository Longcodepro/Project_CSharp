using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Playlist.Commands.RemoveTrackFromPlaylist;

/// <summary>
/// Xóa một media khỏi playlist và cập nhật lại thứ tự.
/// </summary>
public sealed class RemoveTrackFromPlaylistCommandHandler : IRequestHandler<RemoveTrackFromPlaylistCommand, Unit>
{
    private readonly IPlaylistRepository _playlistRepository;

    /// <summary>
    /// Khởi tạo handler xóa track khỏi playlist.
    /// </summary>
    /// <param name="playlistRepository">Repository xử lý truy cập database cho Playlist.</param>
    public RemoveTrackFromPlaylistCommandHandler(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    /// <summary>
    /// Xóa track khỏi playlist nếu người dùng là chủ sở hữu.
    /// </summary>
    /// <param name="command">Command chứa PlaylistId và MediaItemId cần xóa.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <exception cref="DomainException">
    /// Ném ra khi Playlist không tồn tại hoặc Track không có trong Playlist.
    /// </exception>
    /// <exception cref="ForbiddenAccessException">Ném ra khi user không phải chủ playlist.</exception>
    public async Task<Unit> Handle(RemoveTrackFromPlaylistCommand command, CancellationToken cancellationToken)
    {
        var playlist = await _playlistRepository.GetByIdAsync(command.PlaylistId, cancellationToken)
            ?? throw new DomainException("Không tìm thấy playlist.");

        if (playlist.UserId != command.UserId)
            throw new ForbiddenAccessException("Bạn không có quyền xóa bài hát khỏi playlist này.");

        var tracks = (await _playlistRepository.GetPlaylistTracksAsync(command.PlaylistId, cancellationToken)).ToList();
        var trackToRemove = tracks.FirstOrDefault(t => t.MediaItemId == command.MediaItemId)
            ?? throw new DomainException("Bài hát không tồn tại trong playlist.");

        var removedOrder = trackToRemove.TrackOrder;

        await _playlistRepository.RemoveTrackAsync(command.PlaylistId, command.MediaItemId, cancellationToken);

        await _playlistRepository.ShiftTrackOrdersAsync(
            command.PlaylistId,
            removedOrder + 1,
            -1,
            cancellationToken);

        return Unit.Value;
    }
}
