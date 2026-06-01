namespace TuneVault.Application.DTOs.Share;

public sealed record ShareMediaRequestDto(string ReceiverId, string SharedItemId, string ShareType);
