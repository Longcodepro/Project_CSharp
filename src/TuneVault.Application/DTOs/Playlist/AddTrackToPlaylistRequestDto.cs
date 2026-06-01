namespace TuneVault.Application.DTOs.Playlist;

public sealed record AddTrackToPlaylistRequestDto(string PlaylistId, string MediaItemId, int TrackOrder);
