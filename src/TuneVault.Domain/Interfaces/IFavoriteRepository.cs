using TuneVault.Domain.Entities;
using TuneVault.Domain.Enums;

namespace TuneVault.Domain.Interfaces;

/// <summary>
/// Định nghĩa các thao tác truy cập dữ liệu cho chức năng yêu thích và cảm xúc của người dùng đối với media trong TuneVault.
/// Interface này giúp tầng ứng dụng làm việc với Favorite mà không phụ thuộc trực tiếp vào cách lưu trữ dữ liệu.
/// </summary>
public interface IFavoriteRepository
{
    // =========================================================================
    // QUERIES
    // =========================================================================

    /// <summary>
    /// Lấy một bản ghi Favorite theo UserId và MediaItemId.
    /// </summary>
    /// <param name="userId">Mã định danh người dùng.</param>
    /// <param name="mediaItemId">Mã định danh media.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Entity <see cref="Favorite"/> hoặc <c>null</c> nếu không tìm thấy.</returns>
    Task<Favorite?> GetByUserIdAndMediaItemIdAsync(string userId, string mediaItemId, CancellationToken ct = default);

    /// <summary>
    /// Lấy danh sách các bản ghi media item mà người dùng đã thích.
    /// </summary>
    /// <param name="userId">Mã định danh người dùng.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Danh sách các <see cref="Favorite"/> của người dùng.</returns>
    Task<IReadOnlyCollection<Favorite>> GetByUserIdAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra media item còn hoạt động trước khi người dùng yêu thích.
    /// </summary>
    /// <param name="mediaItemId">Mã định danh media.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>True nếu media tồn tại và còn hoạt động.</returns>
    Task<bool> MediaItemExistsAsync(string mediaItemId, CancellationToken ct = default);

    // =========================================================================
    // COMMANDS
    // =========================================================================

    /// <summary>
    /// Thêm một bản ghi Favorite mới vào database.
    /// </summary>
    /// <param name="favorite">Entity Favorite cần thêm.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    Task AddAsync(Favorite favorite, CancellationToken ct = default);

    /// <summary>
    /// Cập nhật thông tin của một bản ghi Favorite trong database.
    /// </summary>
    /// <param name="favorite">Entity Favorite với các thông tin đã thay đổi.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    Task UpdateAsync(Favorite favorite, CancellationToken ct = default);

    /// <summary>
    /// Xóa một bản ghi Favorite khỏi database.
    /// </summary>
    /// <param name="id">Mã định danh của bản ghi Favorite cần xóa.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    Task RemoveAsync(string id, CancellationToken ct = default);
}
