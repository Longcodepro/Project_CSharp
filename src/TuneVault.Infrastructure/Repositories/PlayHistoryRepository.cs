using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.DAO;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// Repository triển khai các thao tác ghi nhận và truy vấn lịch sử nghe nhạc của người dùng.
/// Lớp này sử dụng InteractionDAO để lưu lượt phát và ánh xạ dữ liệu phát gần đây sang entity PlayHistory.
/// </summary>
public sealed class PlayHistoryRepository : IPlayHistoryRepository
{
    private readonly InteractionDAO _interactionDao;

    /// <summary>
    /// Khởi tạo một instance mới của PlayHistoryRepository với DAO xử lý dữ liệu tương tác.
    /// </summary>
    public PlayHistoryRepository(InteractionDAO interactionDao)
    {
        _interactionDao = interactionDao;
    }

    /// <summary>
    /// Ghi nhận một lượt phát media vào lịch sử nghe của người dùng.
    /// </summary>
    public async Task RecordAsync(PlayHistory playHistory, CancellationToken cancellationToken = default)
    {
        await _interactionDao.AddPlayHistoryAsync(
            playHistory.UserId,
            playHistory.MediaItemId);
    }

    /// <summary>
    /// Lấy danh sách các media item được nghe gần đây của một người dùng.
    /// </summary>
    public async Task<IReadOnlyCollection<PlayHistory>> GetRecentByUserIdAsync(Guid userId, int take = 10, CancellationToken cancellationToken = default)
    {
        var userIdValue = RepositoryMappingHelper.ToDatabaseId(userId);
        var rows = await _interactionDao.GetRecentPlayedMediaAsync(userIdValue, take);

        var order = 1;
        return rows.Select(row => MapPlayHistory(row, userIdValue, order++)).ToList();
    }

    /// <summary>
    /// Ánh xạ một dòng dữ liệu phát gần đây thành entity PlayHistory của tầng Domain.
    /// </summary>
    private static PlayHistory MapPlayHistory(object row, string userId, int historyOrder)
    {
        var mediaItemId = RepositoryMappingHelper.ReadString(row, "MediaItemId");
        if (string.IsNullOrWhiteSpace(mediaItemId))
            mediaItemId = RepositoryMappingHelper.ReadString(row, "Id");

        var playHistoryId = RepositoryMappingHelper.ReadString(row, "PlayHistoryId");
        var stoppedAt = RepositoryMappingHelper.ReadNullableDateTime(row, "StoppedAt");

        return RepositoryMappingHelper.CreateEntity<PlayHistory>(
            (nameof(PlayHistory.Id), playHistoryId),
            (nameof(PlayHistory.UserId), userId),
            (nameof(PlayHistory.MediaItemId), mediaItemId),
            (nameof(PlayHistory.HistoryOrder), historyOrder),
            (nameof(PlayHistory.StoppedAt), stoppedAt));
    }
}
