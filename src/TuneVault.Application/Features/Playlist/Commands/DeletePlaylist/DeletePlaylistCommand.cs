using MediatR;

namespace TuneVault.Application.Features.Playlist.Commands.DeletePlaylist;

/// <summary>
/// COMMAND - XÓA PLAYLIST (Application Layer)
/// ===========================================
/// Mục đích: Đóng gói thông tin cần thiết để xóa một Playlist.
/// 
/// Sử dụng:
/// - PlaylistController.Delete() tạo Command này và gửi cho DeletePlaylistCommandHandler
/// </summary>
/// <param name="PlaylistId">Mã định danh của Playlist cần xóa.</param>
public sealed record DeletePlaylistCommand(string PlaylistId, string UserId) : IRequest<Unit>;
