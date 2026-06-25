namespace TuneVault.Application.Features.Album.DTOs;

/// <summary>
/// DTO album công khai dành cho người xem bên ngoài.
/// </summary>
public sealed record AlbumPublicDto(
    string Id,
    string ArtistId,
    string Title,
    string? Description,
    string? CoverImageUrl,
    bool IsPublic,
    string? ContentType,
    DateTime? ReleaseDate,
    DateTime CreatedAt,
    IReadOnlyCollection<AlbumTrackDto> Tracks);
