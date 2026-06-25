namespace TuneVault.Application.Features.Share.DTOs;

/// <summary>
/// DTO request dùng để chia sẻ media, video hoặc playlist cho user khác.
/// </summary>
/// <param name="ReceiverId">Mã người dùng nhận chia sẻ.</param>
/// <param name="SharedItemId">Mã media hoặc playlist được chia sẻ.</param>
/// <param name="ShareType">Loại chia sẻ: Track, Media, Video, Song hoặc Playlist.</param>
/// <param name="Message">Lời nhắn tùy chọn gửi kèm nội dung chia sẻ.</param>
public sealed record ShareMediaRequestDto(
    string ReceiverId,
    string SharedItemId,
    string ShareType,
    string? Message);
