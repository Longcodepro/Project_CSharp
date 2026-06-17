namespace TuneVault.Application.Features.Playlist.DTOs;

/// <summary>
/// DTO request dùng để thêm track vào playlist.
/// </summary>
/// <param name="MediaItemId">Mã media item muốn thêm.</param>
public sealed record AddTrackToPlaylistRequestDto(string MediaItemId);
