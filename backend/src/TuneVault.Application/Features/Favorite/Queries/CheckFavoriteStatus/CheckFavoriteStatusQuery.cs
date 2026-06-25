using MediatR;
using TuneVault.Domain.Enums;

namespace TuneVault.Application.Features.Favorite.Queries.CheckFavoriteStatus;

/// <summary>
/// Query lấy cảm xúc hiện tại của người dùng với media, album hoặc playlist.
/// </summary>
/// <param name="UserId">Mã người dùng hiện tại.</param>
/// <param name="TargetId">Mã đối tượng cần kiểm tra.</param>
/// <param name="TargetType">Loại đối tượng cần kiểm tra.</param>
public record CheckFavoriteStatusQuery(
    string UserId,
    string TargetId,
    FavoriteTargetType TargetType) : IRequest<FavoriteReaction?>;
