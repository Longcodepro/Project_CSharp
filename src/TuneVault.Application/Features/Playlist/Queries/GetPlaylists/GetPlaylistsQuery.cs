namespace TuneVault.Application.Features.Playlist.Queries.GetPlaylists;

/// <summary>
/// Query dành cho việc lấy danh sách playlist của một user.
/// </summary>
/// <param name="OwnerId">Mã user sở hữu playlist.</param>
public sealed record GetPlaylistsQuery(string OwnerId);
