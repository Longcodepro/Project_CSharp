using TuneVault.Domain.Entities;
using TuneVault.Domain.Enums;

namespace TuneVault.Domain.Interfaces;

/// <summary>
/// Định nghĩa các thao tác truy cập dữ liệu cho chức năng yêu thích và cảm xúc của người dùng đối với media, album và playlist trong TuneVault.
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
    /// Lấy một bản ghi Favorite theo UserId, TargetId và TargetType.
    /// </summary>
    /// <param name="userId">Mã định danh người dùng.</param>
    /// <param name="targetId">Mã định danh media, album hoặc playlist.</param>
    /// <param name="targetType">Loại đối tượng được tương tác.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Entity <see cref="Favorite"/> hoặc <c>null</c> nếu không tìm thấy.</returns>
    Task<Favorite?> GetByUserIdAndTargetAsync(
        string userId,
        string targetId,
        FavoriteTargetType targetType,
        CancellationToken ct = default);

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

    /// <summary>
    /// Kiểm tra album hoặc playlist tồn tại và còn khả dụng với người dùng hiện tại.
    /// </summary>
    /// <param name="targetId">Mã album hoặc playlist.</param>
    /// <param name="targetType">Loại target cần kiểm tra.</param>
    /// <param name="userId">Mã người dùng hiện tại để cho phép owner tương tác với private playlist/album.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>True nếu target tồn tại và có thể tương tác.</returns>
    Task<bool> TargetExistsAsync(
        string targetId,
        FavoriteTargetType targetType,
        string userId,
        CancellationToken ct = default);

    /// <summary>
    /// Đếm tổng số lượt thể hiện cảm xúc của một media, album hoặc playlist.
    /// </summary>
    /// <param name="targetId">Mã media, album hoặc playlist.</param>
    /// <param name="targetType">Loại đối tượng được đếm.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Tổng số dòng reaction đang được lưu trong Favorites.</returns>
    Task<int> CountReactionsAsync(
        string targetId,
        FavoriteTargetType targetType,
        CancellationToken ct = default);

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
