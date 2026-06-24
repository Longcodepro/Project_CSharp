namespace TuneVault.Application.Features.Album.DTOs;

/// <summary>
/// DTO chi tiết album dành cho owner.
/// </summary>
public sealed record AlbumDto(
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

/// <summary>
/// DTO đại diện một track trong album.
/// </summary>
public sealed record AlbumTrackDto(
    string MediaItemId,
    int TrackOrder,
    DateTime AddedAt);
