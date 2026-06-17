using MediatR;
using TuneVault.Application.Common;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Interfaces;
using TuneVault.Domain.Exceptions;
using FavoriteEntity = TuneVault.Domain.Entities.Favorite;

namespace TuneVault.Application.Features.Favorite.Commands.ToggleFavorite;

/// <summary>
/// Xử lý command để thêm, cập nhật hoặc xóa trạng thái yêu thích của một bài hát.
/// </summary>
public sealed class ToggleFavoriteCommandHandler : IRequestHandler<ToggleFavoriteCommand, ApiResponse<bool>>
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public ToggleFavoriteCommandHandler(IFavoriteRepository favoriteRepository, ICurrentUserContext currentUserContext)
    {
        _favoriteRepository = favoriteRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<ApiResponse<bool>> Handle(ToggleFavoriteCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.MediaItemId))
        {
            throw new DomainException("Mã media không được để trống.");
        }

        if (!Enum.IsDefined(typeof(FavoriteReaction), request.Reaction))
        {
            throw new DomainException("Loại cảm xúc không hợp lệ.");
        }

        var userId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("Bạn cần đăng nhập để thực hiện thao tác này.");
        }

        var mediaExists = await _favoriteRepository.MediaItemExistsAsync(request.MediaItemId, ct);
        if (!mediaExists)
        {
            throw new DomainException("Không tìm thấy media hoặc media hiện không khả dụng để tương tác.");
        }

        var existingFavorite = await _favoriteRepository.GetByUserIdAndMediaItemIdAsync(userId, request.MediaItemId, ct);

        if (request.Reaction == FavoriteReaction.Remove)
        {
            if (existingFavorite is not null)
            {
                await _favoriteRepository.RemoveAsync(existingFavorite.Id, ct);
                return ApiResponse<bool>.Ok(true, "Đã xóa cảm xúc khỏi media.");
            }

            return ApiResponse<bool>.Ok(true, "Media này chưa có cảm xúc để xóa.");
        }

        if (existingFavorite is null)
        {
            var newFavorite = new FavoriteEntity("FV00", userId, request.MediaItemId, request.Reaction);
            await _favoriteRepository.AddAsync(newFavorite, ct);
            return ApiResponse<bool>.Ok(true, $"Đã thêm cảm xúc {request.Reaction} cho media.");
        }

        if (existingFavorite.Reaction == request.Reaction)
        {
            return ApiResponse<bool>.Ok(true, $"Media này đã có cảm xúc {request.Reaction}.");
        }

        existingFavorite.UpdateReaction(request.Reaction);
        await _favoriteRepository.UpdateAsync(existingFavorite, ct);
        return ApiResponse<bool>.Ok(true, $"Đã cập nhật cảm xúc thành {request.Reaction}.");
    }
}
