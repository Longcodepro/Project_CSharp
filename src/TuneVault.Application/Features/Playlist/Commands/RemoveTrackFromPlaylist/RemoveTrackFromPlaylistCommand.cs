namespace TuneVault.Application.Features.Playlist.Commands.RemoveTrackFromPlaylist;

/// <summary>
/// Command dành cho chức năng xoá track khỏi playlist.
/// </summary>
/// <param name="PlaylistId">Mã playlist chứa track.</param>
/// <param name="MediaItemId">Mã media item cần xoá.</param>
public sealed record RemoveTrackFromPlaylistCommand(string PlaylistId, string MediaItemId);
