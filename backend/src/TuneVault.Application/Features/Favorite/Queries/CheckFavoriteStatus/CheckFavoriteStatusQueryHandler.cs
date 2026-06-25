using MediatR;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Favorite.Queries.CheckFavoriteStatus;

/// <summary>
/// Handler lấy reaction hiện tại của người dùng với media, album hoặc playlist.
/// </summary>
public sealed class CheckFavoriteStatusQueryHandler : IRequestHandler<CheckFavoriteStatusQuery, FavoriteReaction?>
{
    private readonly IFavoriteRepository _favoriteRepository;

    /// <summary>
    /// Khởi tạo handler với repository favorite.
    /// </summary>
    /// <param name="favoriteRepository">Repository đọc dữ liệu cảm xúc.</param>
    public CheckFavoriteStatusQueryHandler(IFavoriteRepository favoriteRepository)
    {
        _favoriteRepository = favoriteRepository;
    }

    /// <summary>
    /// Trả reaction hiện tại nếu có, ngược lại trả null.
    /// </summary>
    /// <param name="request">Query chứa user id, target id và target type.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Reaction hiện tại hoặc null nếu chưa thể hiện cảm xúc.</returns>
    public async Task<FavoriteReaction?> Handle(CheckFavoriteStatusQuery request, CancellationToken cancellationToken)
    {
        var favorite = await _favoriteRepository.GetByUserIdAndTargetAsync(
            request.UserId,
            request.TargetId,
            request.TargetType,
            cancellationToken);

        return favorite?.Reaction;
    }
}
