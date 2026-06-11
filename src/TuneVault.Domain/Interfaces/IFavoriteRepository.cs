using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

/// <summary>
/// Định nghĩa các thao tác truy cập dữ liệu cho chức năng yêu thích và cảm xúc của người dùng đối với media trong TuneVault.
/// Interface này giúp tầng ứng dụng làm việc với Favorite mà không phụ thuộc trực tiếp vào cách lưu trữ dữ liệu.
/// </summary>
public interface IFavoriteRepository
{
    /// <summary>
    /// Kiểm tra một media item có đang được người dùng đánh dấu là yêu thích hay không.
    /// </summary>
    Task<bool> IsFavoriteAsync(Guid userId, Guid mediaItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách các bản ghi media item mà người dùng đã thích.
    /// </summary>
    Task<IReadOnlyCollection<Favorite>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách các bản ghi media item mà người dùng đã đánh dấu không thích.
    /// </summary>
    Task<IReadOnlyCollection<Favorite>> GetDislikedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bật hoặc tắt trạng thái yêu thích dạng Like cho một media item của người dùng.
    /// Nếu media item đã được Like thì thao tác này sẽ xóa Like; nếu chưa Like thì sẽ thêm Like.
    /// </summary>
    Task ToggleAsync(Guid userId, Guid mediaItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thiết lập cảm xúc yêu thích cụ thể của người dùng cho một media item.
    /// </summary>
    Task SetReactionAsync(Guid userId, Guid mediaItemId, FavoriteReaction reaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa trạng thái yêu thích hoặc cảm xúc đã lưu của người dùng đối với một media item.
    /// </summary>
    Task RemoveAsync(Guid userId, Guid mediaItemId, CancellationToken cancellationToken = default);
}
