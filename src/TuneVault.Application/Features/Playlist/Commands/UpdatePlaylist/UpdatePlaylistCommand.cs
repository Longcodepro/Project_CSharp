using MediatR;
using TuneVault.Application.Features.Playlist.DTOs;

namespace TuneVault.Application.Features.Playlist.Commands.UpdatePlaylist;

/// <summary>
/// Command cập nhật metadata và trạng thái public/private của playlist.
/// </summary>
/// <param name="PlaylistId">Mã playlist cần cập nhật.</param>
/// <param name="UserId">Mã người dùng đang thực hiện thao tác.</param>
/// <param name="Request">Payload cập nhật playlist.</param>
public sealed record UpdatePlaylistCommand(
    string PlaylistId,
    string UserId,
    UpdatePlaylistRequestDto Request) : IRequest<PlaylistDto>;
