using MediatR;

namespace TuneVault.Application.Features.Playlist.Commands.DeletePlaylist;

/// <summary>
/// Yêu cầu xóa playlist của người dùng.
/// </summary>
/// <param name="PlaylistId">Mã định danh của Playlist cần xóa.</param>
public sealed record DeletePlaylistCommand(string PlaylistId, string UserId) : IRequest<Unit>;
