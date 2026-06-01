namespace TuneVault.Application.DTOs.Playlist;

public sealed record PlaylistDto(
    string Id,
    string OwnerId,
    string Title,
    string? CoverImgUrl,
    bool IsPublic,
    DateTime CreatedAt);
