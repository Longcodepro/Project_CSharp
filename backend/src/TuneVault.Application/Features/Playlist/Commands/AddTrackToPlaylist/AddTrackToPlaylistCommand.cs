using MediatR;
using TuneVault.Application.Features.Playlist.DTOs;

namespace TuneVault.Application.Features.Playlist.Commands.AddTrackToPlaylist;

/// <summary>
/// Command dành cho chức năng thêm track vào playlist.
/// </summary>
/// <param name="PlaylistId">Mã playlist cần thêm track.</param>
/// <param name="UserId">Mã người dùng đang thực hiện thao tác.</param>
/// <param name="Request">Payload chứa media item cần thêm vào playlist.</param>
public sealed record AddTrackToPlaylistCommand(
    string PlaylistId,
    string UserId,
    AddTrackToPlaylistRequestDto Request) : IRequest<Unit>;
