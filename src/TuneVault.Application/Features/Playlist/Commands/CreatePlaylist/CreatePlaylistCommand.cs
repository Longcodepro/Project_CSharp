using TuneVault.Application.DTOs.Playlist;

namespace TuneVault.Application.Features.Playlist.Commands.CreatePlaylist;

/// <summary>
/// Command dành cho chức năng tạo playlist.
/// </summary>
/// <param name="OwnerId">Mã user tạo playlist.</param>
/// <param name="Request">Payload chứa tiêu đề, ảnh bìa và quyền công khai.</param>
public sealed record CreatePlaylistCommand(string OwnerId, CreatePlaylistRequestDto Request);
