namespace TuneVault.Application.Features.Share.DTOs;

public sealed record SharedItemDto(
    string Id,
    string SenderId,
    string ReceiverId,
    string ShareType,
    string SharedItemId,
    DateTime SharedAt,
    bool IsRead);