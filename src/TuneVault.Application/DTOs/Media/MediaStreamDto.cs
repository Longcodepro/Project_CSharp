namespace TuneVault.Application.DTOs.Media;

public sealed record MediaStreamDto(string MediaId, string FilePath, string ContentType, bool SupportsRange);
