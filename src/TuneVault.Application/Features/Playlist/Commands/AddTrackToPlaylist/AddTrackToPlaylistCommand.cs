using TuneVault.Application.DTOs.Playlist;

namespace TuneVault.Application.Features.Playlist.Commands.AddTrackToPlaylist;

/// <summary>
/// Command dành cho chức năng thêm track vào playlist.
/// </summary>
/// <param name="Request">Payload chứa playlistId, mediaItemId và trackOrder.</param>
public sealed record AddTrackToPlaylistCommand(AddTrackToPlaylistRequestDto Request);
