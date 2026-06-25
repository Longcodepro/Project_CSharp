using MediatR;
using TuneVault.Application.Features.Favorite.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Favorite.Queries.GetFavorites;

/// <summary>
/// Handler lấy danh sách media người dùng đã thể hiện cảm xúc và trả về response gọn.
/// </summary>
public sealed class GetFavoritesQueryHandler : IRequestHandler<GetFavoritesQuery, List<FavoriteSummaryDto>>
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IMediaRepository _mediaRepository;

    /// <summary>
    /// Khởi tạo handler với repository favorite và media.
    /// </summary>
    /// <param name="favoriteRepository">Repository đọc dữ liệu cảm xúc của người dùng.</param>
    /// <param name="mediaRepository">Repository đọc thông tin media để lấy tên bài.</param>
    public GetFavoritesQueryHandler(IFavoriteRepository favoriteRepository, IMediaRepository mediaRepository)
    {
        _favoriteRepository = favoriteRepository;
        _mediaRepository = mediaRepository;
    }

    /// <summary>
    /// Trả về tên media và reaction đã chọn, không trả toàn bộ metadata media.
    /// </summary>
    /// <param name="request">Query chứa user id cần lấy danh sách cảm xúc.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách media kèm cảm xúc đã chọn.</returns>
    public async Task<List<FavoriteSummaryDto>> Handle(GetFavoritesQuery request, CancellationToken cancellationToken)
    {
        var favoriteItems = await _favoriteRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        var result = new List<FavoriteSummaryDto>();

        foreach (var favorite in favoriteItems)
        {
            var mediaItem = await _mediaRepository.GetByIdAsync(favorite.TargetId, cancellationToken);
            if (mediaItem is null)
                continue;

            result.Add(new FavoriteSummaryDto(mediaItem.Title, favorite.Reaction));
        }

        return result;
    }
}
