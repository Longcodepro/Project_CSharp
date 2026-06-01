namespace TuneVault.Application.DTOs.Playlist;

public sealed record CreatePlaylistRequestDto(string Title, bool IsPublic, string? CoverImgUrl);
