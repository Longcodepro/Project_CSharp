namespace TuneVault.Application.Features.Share.DTOs;

/// <summary>
/// DTO cho số lượng chia sẻ chưa đọc của một người dùng.
/// </summary>
public sealed record UnreadShareCountDto(
    string ReceiverId,
    int UnreadCount);