namespace TuneVault.Application.Features.Playlist.DTOs;

/// <summary>
/// DTO công khai dùng khi người xem bên ngoài mở một playlist public.
/// Không chứa các cờ thao tác chỉ dành cho owner.
/// </summary>
public sealed record PlaylistPublicDto(
    string Id,
    string OwnerId,
    string Title,
    string? Description,
    string? CoverImgUrl,
    bool IsPublic,
    string? ContentType,
    DateTime? ReleaseDate,
    DateTime CreatedAt,
    IReadOnlyCollection<PlaylistTrackDto> Tracks
);
