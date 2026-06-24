namespace TuneVault.Application.Features.History.DTOs;

/// <summary>
/// Request lưu vị trí người dùng dừng phát media.
/// </summary>
/// <param name="StoppedAt">Vị trí dừng phát theo giây trong media.</param>
public sealed record RecordPlaybackStopRequestDto(int? StoppedAt);
