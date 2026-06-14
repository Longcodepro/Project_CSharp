namespace TuneVault.Application.DTOs.Playlist;

/// <summary>
/// DTO request dùng để thêm track vào playlist.
/// </summary>
/// <param name="PlaylistId">Mã playlist cần thêm track.</param>
/// <param name="MediaItemId">Mã media item muốn thêm.</param>
/// <param name="TrackOrder">Vị trí track trong playlist.</param>
public sealed record AddTrackToPlaylistRequestDto(string PlaylistId, string MediaItemId, int TrackOrder);