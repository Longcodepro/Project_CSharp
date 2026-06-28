namespace TuneVault.Application.Features.Share.DTOs;

public sealed record SharedItemDto(
    string Id,
    string SenderId,
    string? SenderIdDisplay,
    string? SenderDisplayName,
    string? SenderAvatarUrl,
    string ReceiverId,
    string? ReceiverIdDisplay,
    string? ReceiverDisplayName,
    string? ReceiverAvatarUrl,
    string ShareType,
    string SharedItemId,
    string? ItemTitle,
    string? ItemCoverImageUrl,
    string? Message,
    DateTime SharedAt,
    bool IsRead);
