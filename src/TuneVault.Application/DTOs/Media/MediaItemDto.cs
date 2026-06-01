namespace TuneVault.Application.DTOs.Media;

public sealed record MediaItemDto(
    string Id,
    string OwnerId,
    string Title,
    string? Description,
    string MediaUrl,
    string? CoverImgUrl,
    string? CanvasUrl,
    float Duration,
    string Type,
    string Genre,
    bool IsPublic,
    DateTime UploadedAt,
    DateTime ReleaseDate,
    int ViewCount);
