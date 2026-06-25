namespace TuneVault.Application.Features.History.DTOs;

/// <summary>
/// DTO gọn để frontend biết media nào cần phát tiếp, kèm vị trí dừng theo giây.
/// </summary>
/// <param name="MediaId">Mã media trong lịch sử.</param>
/// <param name="Title">Tên media.</param>
/// <param name="StoppedAt">Vị trí dừng phát gần nhất theo giây.</param>
public sealed record HistoryResumeDto(string MediaId, string Title, int? StoppedAt);
