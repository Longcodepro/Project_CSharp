namespace TuneVault.Application.Features.Playlist.Commands.RemoveTrackFromPlaylist;

public sealed record RemoveTrackFromPlaylistCommand(string PlaylistId, string MediaItemId);
