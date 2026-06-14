namespace TuneVault.Application.Features.Playlist.DTOs;

public sealed record PlaylistDto(
    string Id,
    string OwnerId,
    string Title,
    string? CoverImgUrl,
    bool IsPublic,
    DateTime CreatedAt);