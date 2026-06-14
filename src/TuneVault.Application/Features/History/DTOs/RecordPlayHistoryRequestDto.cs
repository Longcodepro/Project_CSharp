namespace TuneVault.Application.Features.History.DTOs;

public sealed record RecordPlayHistoryRequestDto(string MediaItemId, float? StoppedAt);