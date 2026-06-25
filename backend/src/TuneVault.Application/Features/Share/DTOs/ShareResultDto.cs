namespace TuneVault.Application.Features.Share.DTOs;

/// <summary>
/// DTO cho kết quả của một thao tác chia sẻ media.
/// </summary>
public sealed record ShareResultDto(
    string ShareId,
    string SenderId,
    string ReceiverId,
    string ShareType,
    string SharedItemId);