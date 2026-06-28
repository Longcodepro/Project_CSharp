using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Playlist.Commands.DeletePlaylist;

/// <summary>
/// Xóa playlist của người dùng hiện tại.
/// </summary>
public sealed class DeletePlaylistCommandHandler : IRequestHandler<DeletePlaylistCommand, Unit>
{
    private readonly IPlaylistRepository _playlistRepository;

    /// <summary>
    /// Khởi tạo handler xóa playlist.
    /// </summary>
    /// <param name="playlistRepository">Repository xử lý truy cập database cho Playlist.</param>
    public DeletePlaylistCommandHandler(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    /// <summary>
    /// Xóa playlist nếu người dùng là chủ sở hữu.
    /// </summary>
    /// <param name="command">Command chứa PlaylistId cần xóa.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <exception cref="DomainException">Ném ra khi Playlist không tồn tại.</exception>
    /// <exception cref="ForbiddenAccessException">Ném ra khi user không phải chủ playlist.</exception>
    public async Task<Unit> Handle(DeletePlaylistCommand command, CancellationToken cancellationToken)
    {
        var playlist = await _playlistRepository.GetByIdAsync(command.PlaylistId, cancellationToken)
            ?? throw new DomainException("Không tìm thấy playlist.");

        if (playlist.UserId != command.UserId)
            throw new ForbiddenAccessException("Bạn không có quyền xóa playlist này.");

        playlist.Delete();

        await _playlistRepository.DeleteAsync(command.PlaylistId, cancellationToken);
        return Unit.Value;
    }
}
