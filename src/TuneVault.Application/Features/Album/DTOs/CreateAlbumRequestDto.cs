namespace TuneVault.Application.Features.Album.DTOs;

/// <summary>
/// Payload tạo album mới.
/// </summary>
public sealed record CreateAlbumRequestDto(
    string Title,
    string? Description,
    string? CoverImageUrl,
    bool IsPublic,
    string? ContentType,
    DateTime? ReleaseDate);
