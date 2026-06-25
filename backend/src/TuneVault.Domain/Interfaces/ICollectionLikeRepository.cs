using TuneVault.Domain.Entities;
using TuneVault.Domain.Enums;

namespace TuneVault.Domain.Interfaces;

/// <summary>
/// Định nghĩa truy cập dữ liệu cho lượt thích album và playlist.
/// </summary>
public interface ICollectionLikeRepository
{
    /// <summary>
    /// Lấy lượt thích hiện có của một user với album hoặc playlist.
    /// </summary>
    Task<CollectionLike?> GetByUserAndTargetAsync(
        string userId,
        string targetId,
        CollectionLikeTargetType targetType,
        CancellationToken ct = default);

    /// <summary>
    /// Lấy các album/playlist người dùng thích gần nhất.
    /// </summary>
    Task<IReadOnlyCollection<CollectionLikeSummary>> GetRecentByUserAsync(
        string userId,
        int limit,
        CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra album hoặc playlist có tồn tại và người dùng hiện tại được phép xem hay không.
    /// </summary>
    Task<bool> TargetExistsAsync(
        string targetId,
        CollectionLikeTargetType targetType,
        string userId,
        CancellationToken ct = default);

    /// <summary>
    /// Thêm lượt thích mới.
    /// </summary>
    Task AddAsync(CollectionLike like, CancellationToken ct = default);

    /// <summary>
    /// Xóa lượt thích theo mã.
    /// </summary>
    Task RemoveAsync(string id, CancellationToken ct = default);
}

/// <summary>
/// Dữ liệu gọn để render album/playlist đã thích ở frontend.
/// </summary>
public sealed record CollectionLikeSummary(
    string Id,
    string TargetId,
    CollectionLikeTargetType TargetType,
    string Title,
    string? Description,
    string? CoverImageUrl,
    DateTime LikedAt);
