using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

/// <summary>
/// Định nghĩa các thao tác truy cập dữ liệu cho lịch sử nghe nhạc của người dùng trong TuneVault.
/// Interface này dùng để ghi nhận lượt phát và truy xuất danh sách media đã nghe gần đây.
/// </summary>
public interface IPlayHistoryRepository
{
    // =========================================================================
    // QUERIES
    // =========================================================================

    /// <summary>
    /// Lấy một bản ghi lịch sử nghe theo UserId và MediaItemId.
    /// </summary>
    /// <param name="userId">Mã định danh người dùng.</param>
    /// <param name="mediaItemId">Mã định danh media.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Entity <see cref="PlayHistory"/> hoặc <c>null</c> nếu không tìm thấy.</returns>
    Task<PlayHistory?> GetByUserIdAndMediaItemIdAsync(string userId, string mediaItemId, CancellationToken ct = default);

    /// <summary>
    /// Lấy danh sách các bản ghi lịch sử nghe gần đây của người dùng.
    /// </summary>
    /// <param name="userId">Mã định danh người dùng.</param>
    /// <param name="take">Số lượng bản ghi tối đa cần lấy.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Danh sách các <see cref="PlayHistory"/> của người dùng.</returns>
    Task<IReadOnlyCollection<PlayHistory>> GetRecentByUserIdAsync(string userId, int take = 10, CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra media item còn hoạt động trước khi ghi lịch sử nghe.
    /// </summary>
    /// <param name="mediaItemId">Mã định danh media.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>True nếu media tồn tại và còn hoạt động.</returns>
    Task<bool> MediaItemExistsAsync(string mediaItemId, CancellationToken ct = default);

    // =========================================================================
    // COMMANDS
    // =========================================================================

    /// <summary>
    /// Thêm một bản ghi lịch sử nghe mới vào database.
    /// </summary>
    /// <param name="playHistory">Entity PlayHistory cần thêm.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    Task AddAsync(PlayHistory playHistory, CancellationToken ct = default);

    /// <summary>
    /// Cập nhật thông tin của một bản ghi lịch sử nghe trong database.
    /// </summary>
    /// <param name="playHistory">Entity PlayHistory với các thông tin đã thay đổi.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    Task UpdateAsync(PlayHistory playHistory, CancellationToken ct = default);
}
