using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.DAO;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// Repository triển khai các thao tác lưu trữ và truy vấn dữ liệu Favorite của người dùng.
/// Lớp này chuyển đổi dữ liệu giữa InteractionDAO và entity Favorite trong tầng Domain.
/// </summary>
public sealed class FavoriteRepository : IFavoriteRepository
{
    private readonly InteractionDAO _interactionDao;

    /// <summary>
    /// Khởi tạo một instance mới của FavoriteRepository với DAO xử lý dữ liệu tương tác.
    /// </summary>
    public FavoriteRepository(InteractionDAO interactionDao)
    {
        _interactionDao = interactionDao;
    }

    /// <summary>
    /// Bật hoặc tắt trạng thái Like của người dùng đối với một media item.
    /// Nếu media item hiện đang Like thì sẽ xóa Like; nếu chưa Like thì sẽ lưu trạng thái Like.
    /// </summary>
    public async Task ToggleAsync(Guid userId, Guid mediaItemId, CancellationToken cancellationToken = default)
    {
        var userIdValue = RepositoryMappingHelper.ToDatabaseId(userId);
        var mediaItemIdValue = RepositoryMappingHelper.ToDatabaseId(mediaItemId);

        var currentStatus = await _interactionDao.GetFavoriteStatusAsync(userIdValue, mediaItemIdValue);
        if (string.Equals(currentStatus, FavoriteReaction.Like.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            await _interactionDao.RemoveFavoriteAsync(userIdValue, mediaItemIdValue);
            return;
        }

        await _interactionDao.SetFavoriteStatusAsync(userIdValue, mediaItemIdValue, FavoriteReaction.Like.ToString());
    }

    /// <summary>
    /// Thiết lập một cảm xúc yêu thích cụ thể cho media item của người dùng.
    /// </summary>
    public async Task SetReactionAsync(Guid userId, Guid mediaItemId, FavoriteReaction reaction, CancellationToken cancellationToken = default)
    {
        var userIdValue = RepositoryMappingHelper.ToDatabaseId(userId);
        var mediaItemIdValue = RepositoryMappingHelper.ToDatabaseId(mediaItemId);

        await _interactionDao.SetFavoriteStatusAsync(userIdValue, mediaItemIdValue, reaction.ToString());
    }

    /// <summary>
    /// Xóa trạng thái yêu thích hoặc cảm xúc của người dùng đối với một media item.
    /// </summary>
    public async Task RemoveAsync(Guid userId, Guid mediaItemId, CancellationToken cancellationToken = default)
    {
        var userIdValue = RepositoryMappingHelper.ToDatabaseId(userId);
        var mediaItemIdValue = RepositoryMappingHelper.ToDatabaseId(mediaItemId);

        await _interactionDao.RemoveFavoriteAsync(userIdValue, mediaItemIdValue);
    }

    /// <summary>
    /// Kiểm tra media item có đang được người dùng đánh dấu Like hay không.
    /// </summary>
    public async Task<bool> IsFavoriteAsync(Guid userId, Guid mediaItemId, CancellationToken cancellationToken = default)
    {
        var userIdValue = RepositoryMappingHelper.ToDatabaseId(userId);
        var mediaItemIdValue = RepositoryMappingHelper.ToDatabaseId(mediaItemId);

        var status = await _interactionDao.GetFavoriteStatusAsync(userIdValue, mediaItemIdValue);
        return string.Equals(status, FavoriteReaction.Like.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Lấy danh sách media item đã được người dùng Like và ánh xạ thành entity Favorite.
    /// </summary>
    public async Task<IReadOnlyCollection<Favorite>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userIdValue = RepositoryMappingHelper.ToDatabaseId(userId);
        var rows = await _interactionDao.GetLikedMediaAsync(userIdValue);

        return rows.Select(row => MapFavorite(row, userIdValue, FavoriteReaction.Like)).ToList();
    }

    /// <summary>
    /// Lấy danh sách media item đã bị người dùng đánh dấu Dislike và ánh xạ thành entity Favorite.
    /// </summary>
    public async Task<IReadOnlyCollection<Favorite>> GetDislikedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userIdValue = RepositoryMappingHelper.ToDatabaseId(userId);
        var rows = await _interactionDao.GetDislikedMediaAsync(userIdValue);

        return rows.Select(row => MapFavorite(row, userIdValue, FavoriteReaction.Dislike)).ToList();
    }

    /// <summary>
    /// Ánh xạ một dòng dữ liệu trả về từ DAO thành entity Favorite của tầng Domain.
    /// </summary>
    private static Favorite MapFavorite(object row, string userId, FavoriteReaction defaultReaction)
    {
        var mediaItemId = RepositoryMappingHelper.ReadString(row, "MediaItemId");
        if (string.IsNullOrWhiteSpace(mediaItemId))
            mediaItemId = RepositoryMappingHelper.ReadString(row, "Id");

        var reaction = RepositoryMappingHelper.ReadEnum(row, "LikeStatus", defaultReaction);
        var likedAt = RepositoryMappingHelper.ReadDateTime(row, "LikedAt");

        return RepositoryMappingHelper.CreateEntity<Favorite>(
            (nameof(Favorite.Id), string.Empty),
            (nameof(Favorite.UserId), userId),
            (nameof(Favorite.MediaItemId), mediaItemId),
            (nameof(Favorite.Reaction), reaction),
            (nameof(Favorite.LikedAt), likedAt));
    }
}
