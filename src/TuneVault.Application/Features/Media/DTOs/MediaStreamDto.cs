namespace TuneVault.Application.Features.Media.DTOs;

public sealed record MediaStreamDto(string MediaId, string FilePath, string ContentType, bool SupportsRange);