using MediatR;
using TuneVault.Application.Common;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Interfaces;
using TuneVault.Domain.Exceptions;
using FavoriteEntity = TuneVault.Domain.Entities.Favorite;

namespace TuneVault.Application.Features.Favorite.Commands.ToggleFavorite;

/// <summary>
/// Xử lý command để thêm hoặc xóa trạng thái yêu thích của media, album hoặc playlist.
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
        if (string.IsNullOrWhiteSpace(request.TargetId))
        {
            throw new DomainException("Mã đối tượng cần tương tác không được để trống.");
        }

        if (!Enum.IsDefined(typeof(FavoriteTargetType), request.TargetType))
        {
            throw new DomainException("Loại đối tượng cần tương tác không hợp lệ.");
        }

        var userId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("Bạn cần đăng nhập để thực hiện thao tác này.");
        }

        var targetExists = await _favoriteRepository.TargetExistsAsync(request.TargetId, request.TargetType, userId, ct);
        if (!targetExists)
        {
            throw new DomainException($"Không tìm thấy {GetTargetDisplayName(request.TargetType)} hoặc đối tượng này hiện không khả dụng để tương tác.");
        }

        var existingFavorite = await _favoriteRepository.GetByUserIdAndTargetAsync(
            userId,
            request.TargetId,
            request.TargetType,
            includeInactive: true,
            ct: ct);

        if (!request.IsActive)
        {
            if (existingFavorite is not null && existingFavorite.IsActive)
            {
                existingFavorite.Deactivate();
                await _favoriteRepository.RemoveAsync(existingFavorite.Id, ct);
                return ApiResponse<bool>.Ok(false, $"Đã hủy yêu thích khỏi {GetTargetDisplayName(request.TargetType)}.");
            }

            return ApiResponse<bool>.Ok(false, $"{GetTargetDisplayName(request.TargetType, capitalize: true)} này hiện chưa được yêu thích.");
        }

        if (existingFavorite is null)
        {
            var newFavorite = new FavoriteEntity("FV00", userId, request.TargetId, request.TargetType);
            await _favoriteRepository.AddAsync(newFavorite, ct);
            return ApiResponse<bool>.Ok(true, $"Đã thêm yêu thích cho {GetTargetDisplayName(request.TargetType)}.");
        }

        if (existingFavorite.IsActive)
        {
            return ApiResponse<bool>.Ok(true, $"{GetTargetDisplayName(request.TargetType, capitalize: true)} này đã được yêu thích.");
        }

        existingFavorite.Activate();
        await _favoriteRepository.UpdateAsync(existingFavorite, ct);
        return ApiResponse<bool>.Ok(true, $"Đã khôi phục yêu thích cho {GetTargetDisplayName(request.TargetType)}.");
    }

    private static string GetTargetDisplayName(FavoriteTargetType targetType, bool capitalize = false)
    {
        var name = targetType switch
        {
            FavoriteTargetType.Media => "media",
            FavoriteTargetType.Album => "album",
            FavoriteTargetType.Playlist => "playlist",
            _ => "đối tượng"
        };

        return capitalize
            ? char.ToUpperInvariant(name[0]) + name[1..]
            : name;
    }
}
