using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

/// <summary>
/// Định nghĩa các thao tác truy cập dữ liệu cho lịch sử nghe nhạc của người dùng trong TuneVault.
/// Interface này dùng để ghi nhận lượt phát và truy xuất danh sách media đã nghe gần đây.
/// </summary>
public interface IPlayHistoryRepository
{
    /// <summary>
    /// Ghi nhận một lượt phát media mới vào lịch sử nghe của người dùng.
    /// </summary>
    Task RecordAsync(PlayHistory playHistory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách media item được nghe gần đây của một người dùng.
    /// </summary>
    Task<IReadOnlyCollection<PlayHistory>> GetRecentByUserIdAsync(Guid userId, int take = 10, CancellationToken cancellationToken = default);
}
