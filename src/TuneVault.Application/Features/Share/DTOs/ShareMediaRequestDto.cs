namespace TuneVault.Application.Features.Share.DTOs;

public sealed record ShareMediaRequestDto(string MediaId, string SharedWithUserId, string ShareType);