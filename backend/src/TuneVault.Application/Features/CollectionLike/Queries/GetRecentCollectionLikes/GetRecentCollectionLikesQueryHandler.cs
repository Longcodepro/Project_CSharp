using MediatR;
using TuneVault.Application.Features.CollectionLike.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.CollectionLike.Queries.GetRecentCollectionLikes;

/// <summary>
/// Handler đọc album/playlist đã thích gần nhất để render nhanh ở sidebar.
/// </summary>
public sealed class GetRecentCollectionLikesQueryHandler
    : IRequestHandler<GetRecentCollectionLikesQuery, IReadOnlyCollection<CollectionLikeDto>>
{
    private readonly ICollectionLikeRepository _collectionLikeRepository;
    private readonly ICurrentUserContext _currentUserContext;

    /// <summary>
    /// Khởi tạo handler với repository lượt thích collection và ngữ cảnh user.
    /// </summary>
    public GetRecentCollectionLikesQueryHandler(
        ICollectionLikeRepository collectionLikeRepository,
        ICurrentUserContext currentUserContext)
    {
        _collectionLikeRepository = collectionLikeRepository;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// Trả danh sách tối đa 3 mục theo yêu cầu giao diện sidebar.
    /// </summary>
    public async Task<IReadOnlyCollection<CollectionLikeDto>> Handle(
        GetRecentCollectionLikesQuery request,
        CancellationToken ct)
    {
        var userId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("Bạn cần đăng nhập để thực hiện thao tác này.");

        var limit = Math.Clamp(request.Limit, 1, 10);
        var rows = await _collectionLikeRepository.GetRecentByUserAsync(userId, limit, ct);

        return rows.Select(row => new CollectionLikeDto(
            row.Id,
            row.TargetId,
            row.TargetType,
            row.Title,
            row.Description,
            row.CoverImageUrl,
            row.LikedAt)).ToList();
    }
}
