namespace TuneVault.Application.Features.Playlist.DTOs;

public sealed record CreatePlaylistRequestDto(string Title, bool IsPublic, string? CoverImgUrl);