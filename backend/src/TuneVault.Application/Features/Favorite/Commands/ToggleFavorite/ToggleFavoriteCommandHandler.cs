using MediatR;
using TuneVault.Application.Common;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Interfaces;
using TuneVault.Domain.Exceptions;
using FavoriteEntity = TuneVault.Domain.Entities.Favorite;

namespace TuneVault.Application.Features.Favorite.Commands.ToggleFavorite;

/// <summary>
/// Xử lý command để thêm, cập nhật hoặc xóa trạng thái cảm xúc của media, album hoặc playlist.
/// </summary>
public sealed class ToggleFavoriteCommandHandler : IRequestHandler<ToggleFavoriteCommand, ApiResponse<FavoriteReaction?>>
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public ToggleFavoriteCommandHandler(IFavoriteRepository favoriteRepository, ICurrentUserContext currentUserContext)
    {
        _favoriteRepository = favoriteRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<ApiResponse<FavoriteReaction?>> Handle(ToggleFavoriteCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.TargetId))
        {
            throw new DomainException("Mã đối tượng cần tương tác không được để trống.");
        }

        if (!Enum.IsDefined(typeof(FavoriteTargetType), request.TargetType))
        {
            throw new DomainException("Loại đối tượng cần tương tác không hợp lệ.");
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

        var targetExists = await _favoriteRepository.TargetExistsAsync(request.TargetId, request.TargetType, userId, ct);
        if (!targetExists)
        {
            throw new DomainException($"Không tìm thấy {GetTargetDisplayName(request.TargetType)} hoặc đối tượng này hiện không khả dụng để tương tác.");
        }

        var existingFavorite = await _favoriteRepository.GetByUserIdAndTargetAsync(
            userId,
            request.TargetId,
            request.TargetType,
            ct);

        if (request.Reaction == FavoriteReaction.Remove)
        {
            if (existingFavorite is not null)
            {
                await _favoriteRepository.RemoveAsync(existingFavorite.Id, ct);
                return ApiResponse<FavoriteReaction?>.Ok(null, $"Đã xóa cảm xúc khỏi {GetTargetDisplayName(request.TargetType)}.");
            }

            return ApiResponse<FavoriteReaction?>.Ok(null, $"{GetTargetDisplayName(request.TargetType, capitalize: true)} này chưa có cảm xúc để xóa.");
        }

        if (existingFavorite is null)
        {
            var newFavorite = new FavoriteEntity("FV00", userId, request.TargetId, request.TargetType, request.Reaction);
            await _favoriteRepository.AddAsync(newFavorite, ct);
            return ApiResponse<FavoriteReaction?>.Ok(request.Reaction, $"Đã thêm cảm xúc {request.Reaction} cho {GetTargetDisplayName(request.TargetType)}.");
        }

        if (existingFavorite.Reaction == request.Reaction)
        {
            return ApiResponse<FavoriteReaction?>.Ok(request.Reaction, $"{GetTargetDisplayName(request.TargetType, capitalize: true)} này đã có cảm xúc {request.Reaction}.");
        }

        existingFavorite.UpdateReaction(request.Reaction);
        await _favoriteRepository.UpdateAsync(existingFavorite, ct);
        return ApiResponse<FavoriteReaction?>.Ok(request.Reaction, $"Đã cập nhật cảm xúc thành {request.Reaction}.");
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
