namespace TuneVault.Application.DTOs.Share;

public sealed record SharedItemDto(
    string Id,
    string SenderId,
    string ReceiverId,
    string ShareType,
    string SharedItemId,
    DateTime SharedAt,
    bool IsRead);
