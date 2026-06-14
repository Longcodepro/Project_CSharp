namespace TuneVault.Application.Features.Playlist.DTOs;

public sealed record AddTrackToPlaylistRequestDto(string PlaylistId, string MediaItemId, int TrackOrder);