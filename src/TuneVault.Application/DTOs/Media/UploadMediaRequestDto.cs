namespace TuneVault.Application.DTOs.Media;

public sealed record UploadMediaRequestDto(
    string Title,
    string? Description,
    string Genre,
    bool IsPublic);
