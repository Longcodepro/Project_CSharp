namespace TuneVault.Application.Features.Album.DTOs;

/// <summary>
/// Payload cập nhật thông tin album.
/// </summary>
public sealed record UpdateAlbumRequestDto(
    string Title,
    string? Description,
    string? CoverImageUrl,
    bool IsPublic,
    string? ContentType,
    DateTime? ReleaseDate);
