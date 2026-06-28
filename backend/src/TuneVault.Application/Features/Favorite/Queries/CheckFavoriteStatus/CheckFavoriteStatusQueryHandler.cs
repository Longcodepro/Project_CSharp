using MediatR;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Favorite.Queries.CheckFavoriteStatus;

/// <summary>
/// Handler lấy trạng thái yêu thích hiện tại của người dùng với media, album hoặc playlist.
/// </summary>
public sealed class CheckFavoriteStatusQueryHandler : IRequestHandler<CheckFavoriteStatusQuery, bool>
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
    /// Trả true nếu bản ghi favorite đang active, ngược lại trả false.
    /// </summary>
    /// <param name="request">Query chứa user id, target id và target type.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>true nếu đã yêu thích.</returns>
    public async Task<bool> Handle(CheckFavoriteStatusQuery request, CancellationToken cancellationToken)
    {
        var favorite = await _favoriteRepository.GetByUserIdAndTargetAsync(
            request.UserId,
            request.TargetId,
            request.TargetType,
            ct: cancellationToken);

        return favorite is not null && favorite.IsActive;
    }
}
