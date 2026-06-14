namespace TuneVault.Application.Features.Favorite.Commands;

/// <summary>
/// Interface repository riêng cho SQL Favorite.
/// Để chung trong file này để không cần tạo thêm file interface mới.
/// Dùng string vì database hiện tại dùng Id dạng U001, M001, FV001.
/// </summary>
public interface IFavoriteSqlRepository
{
    Task<bool> IsFavoriteAsync(string userId, string mediaItemId);

    Task<IEnumerable<dynamic>> GetByUserIdAsync(string userId);

    Task ToggleAsync(string userId, string mediaItemId);

    Task SetReactionAsync(string userId, string mediaItemId, string reaction);

    Task RemoveAsync(string userId, string mediaItemId);
}

/// <summary>
/// Command xử lý nghiệp vụ Favorite.
/// Controller gọi command này, command gọi repository.
/// Không viết SQL ở đây.
/// </summary>
public sealed class ToggleFavoriteCommand
{
    private readonly IFavoriteSqlRepository _favoriteRepository;

    public ToggleFavoriteCommand(IFavoriteSqlRepository favoriteRepository)
    {
        _favoriteRepository = favoriteRepository;
    }

    /// <summary>
    /// Toggle Like.
    /// Nếu đã Like thì xóa.
    /// Nếu chưa Like thì thêm Like.
    /// </summary>
    public async Task ToggleAsync(
        string userId,
        string mediaItemId,
        CancellationToken cancellationToken = default)
    {
        ValidateRequired(userId, nameof(userId));
        ValidateRequired(mediaItemId, nameof(mediaItemId));

        await _favoriteRepository.ToggleAsync(
            userId.Trim(),
            mediaItemId.Trim());
    }

    /// <summary>
    /// Set reaction cụ thể: Like, Love, Chill, Energetic...
    /// </summary>
    public async Task SetReactionAsync(
        string userId,
        string mediaItemId,
        string reaction,
        CancellationToken cancellationToken = default)
    {
        ValidateRequired(userId, nameof(userId));
        ValidateRequired(mediaItemId, nameof(mediaItemId));
        ValidateRequired(reaction, nameof(reaction));

        await _favoriteRepository.SetReactionAsync(
            userId.Trim(),
            mediaItemId.Trim(),
            reaction.Trim());
    }

    /// <summary>
    /// Xóa favorite/reaction của user với media item.
    /// </summary>
    public async Task RemoveAsync(
        string userId,
        string mediaItemId,
        CancellationToken cancellationToken = default)
    {
        ValidateRequired(userId, nameof(userId));
        ValidateRequired(mediaItemId, nameof(mediaItemId));

        await _favoriteRepository.RemoveAsync(
            userId.Trim(),
            mediaItemId.Trim());
    }

    /// <summary>
    /// Kiểm tra media item có đang được user favorite không.
    /// </summary>
    public async Task<bool> IsFavoriteAsync(
        string userId,
        string mediaItemId,
        CancellationToken cancellationToken = default)
    {
        ValidateRequired(userId, nameof(userId));
        ValidateRequired(mediaItemId, nameof(mediaItemId));

        return await _favoriteRepository.IsFavoriteAsync(
            userId.Trim(),
            mediaItemId.Trim());
    }

    /// <summary>
    /// Lấy danh sách favorite của user.
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ValidateRequired(userId, nameof(userId));

        return await _favoriteRepository.GetByUserIdAsync(userId.Trim());
    }

    private static void ValidateRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} không được để trống.", parameterName);
    }
}