using TuneVault.Application.Features.Playlist.Commands.DeletePlaylist;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Playlist.Commands.DeletePlaylist;

/// <summary>
/// COMMAND HANDLER - XÓA PLAYLIST (Application Layer)
/// ===================================================
/// Mục đích: Xử lý logic nghiệp vụ xóa toàn bộ một Playlist.
/// 
/// Luồng xử lý:
/// 1. Controller gửi DeletePlaylistCommand
/// 2. Handler lấy Playlist từ Repository (GetByIdAsync)
/// 3. Handler gọi playlist.Delete() — Entity thực thi logic nghiệp vụ xóa
/// 4. Handler gọi Repository xóa Playlist khỏi Database
/// 
/// Lý do gọi Entity.Delete() thay vì gọi Repository trực tiếp:
/// - Đảm bảo toàn vẹn Aggregate Root
/// - Logic nghiệp vụ trước khi xóa tập trung trong Entity
/// </summary>
public sealed class DeletePlaylistCommandHandler
{
    private readonly IPlaylistRepository _playlistRepository;

    /// <summary>
    /// Khởi tạo Handler với Repository được inject qua DI container.
    /// </summary>
    /// <param name="playlistRepository">Repository xử lý truy cập database cho Playlist.</param>
    public DeletePlaylistCommandHandler(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    /// <summary>
    /// Thực thi logic xóa Playlist từ Command.
    /// Gọi Entity.Delete() trước khi xóa khỏi Database.
    /// </summary>
    /// <param name="command">Command chứa PlaylistId cần xóa.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <exception cref="InvalidOperationException">Ném ra khi Playlist không tồn tại.</exception>
    public async Task HandleAsync(DeletePlaylistCommand command, CancellationToken cancellationToken = default)
    {
        // Lấy Playlist từ Database — kiểm tra tồn tại
        var playlist = await _playlistRepository.GetByIdAsync(command.PlaylistId, cancellationToken)
            ?? throw new InvalidOperationException($"Playlist '{command.PlaylistId}' không tồn tại.");

        // Gọi Entity.Delete() — Entity thực thi logic nghiệp vụ trước khi xóa
        playlist.Delete();

        // Gọi Repository xóa Playlist khỏi Database
        await _playlistRepository.DeleteAsync(command.PlaylistId, cancellationToken);
    }
}