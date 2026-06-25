using MediatR;
using TuneVault.Application.Common;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;
using CollectionLikeEntity = TuneVault.Domain.Entities.CollectionLike;

namespace TuneVault.Application.Features.CollectionLike.Commands.ToggleCollectionLike;

/// <summary>
/// Xử lý thêm hoặc xóa lượt thích cho album/playlist của người dùng hiện tại.
/// </summary>
public sealed class ToggleCollectionLikeCommandHandler
    : IRequestHandler<ToggleCollectionLikeCommand, ApiResponse<bool>>
{
    private readonly ICollectionLikeRepository _collectionLikeRepository;
    private readonly ICurrentUserContext _currentUserContext;

    /// <summary>
    /// Khởi tạo handler với repository lượt thích collection và thông tin user hiện tại.
    /// </summary>
    public ToggleCollectionLikeCommandHandler(
        ICollectionLikeRepository collectionLikeRepository,
        ICurrentUserContext currentUserContext)
    {
        _collectionLikeRepository = collectionLikeRepository;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// Toggle lượt thích, trả true nếu sau thao tác đang thích và false nếu đã bỏ thích.
    /// </summary>
    public async Task<ApiResponse<bool>> Handle(ToggleCollectionLikeCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.TargetId))
            throw new DomainException("Mã album hoặc playlist không được để trống.");

        if (!Enum.IsDefined(typeof(CollectionLikeTargetType), request.TargetType))
            throw new DomainException("Loại album/playlist không hợp lệ.");

        var userId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("Bạn cần đăng nhập để thực hiện thao tác này.");

        var targetExists = await _collectionLikeRepository.TargetExistsAsync(
            request.TargetId,
            request.TargetType,
            userId,
            ct);

        if (!targetExists)
            throw new DomainException("Không tìm thấy album/playlist hoặc bạn không có quyền xem mục này.");

        var existingLike = await _collectionLikeRepository.GetByUserAndTargetAsync(
            userId,
            request.TargetId,
            request.TargetType,
            ct);

        if (existingLike is not null)
        {
            await _collectionLikeRepository.RemoveAsync(existingLike.Id, ct);
            return ApiResponse<bool>.Ok(false, "Đã bỏ thích album/playlist.");
        }

        var like = new CollectionLikeEntity("CL00", userId, request.TargetId, request.TargetType);
        await _collectionLikeRepository.AddAsync(like, ct);
        return ApiResponse<bool>.Ok(true, "Đã thích album/playlist.");
    }
}
