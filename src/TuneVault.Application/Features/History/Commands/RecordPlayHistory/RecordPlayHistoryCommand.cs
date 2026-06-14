namespace TuneVault.Application.Features.History.Commands;

/// <summary>
/// Interface repository riêng cho SQL PlayHistory.
/// Dùng string vì database hiện tại dùng Id dạng U001, M001, PH001.
/// </summary>
public interface IPlayHistorySqlRepository
{
    Task<bool> RecordAsync(
        string userId,
        string mediaItemId,
        double? stoppedAt = null);

    Task<IEnumerable<dynamic>> GetRecentByUserIdAsync(
        string userId,
        int limit = 10);
}

/// <summary>
/// Command xử lý nghiệp vụ lịch sử nghe nhạc.
/// Controller gọi command này, command gọi repository.
/// Không viết SQL ở đây.
/// </summary>
public sealed class RecordPlayHistoryCommand
{
    private readonly IPlayHistorySqlRepository _playHistoryRepository;

    public RecordPlayHistoryCommand(IPlayHistorySqlRepository playHistoryRepository)
    {
        _playHistoryRepository = playHistoryRepository;
    }

    /// <summary>
    /// Ghi nhận một lần nghe bài hát của người dùng.
    /// </summary>
    public async Task<bool> RecordAsync(
        string userId,
        string mediaItemId,
        double? stoppedAt = null)
    {
        ValidateRequired(userId, nameof(userId));
        ValidateRequired(mediaItemId, nameof(mediaItemId));

        return await _playHistoryRepository.RecordAsync(
            userId.Trim(),
            mediaItemId.Trim(),
            stoppedAt);
    }

    /// <summary>
    /// Lấy danh sách bài hát nghe gần đây của user.
    /// Theo yêu cầu hiện tại, để chung trong command này để không tạo thêm Query file.
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetRecentAsync(
        string userId,
        int limit = 10)
    {
        ValidateRequired(userId, nameof(userId));

        if (limit <= 0)
            limit = 10;

        return await _playHistoryRepository.GetRecentByUserIdAsync(
            userId.Trim(),
            limit);
    }

    private static void ValidateRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} không được để trống.", parameterName);
    }
}