using MediatR;
using TuneVault.Application.Features.Favorite.DTOs;

namespace TuneVault.Application.Features.Favorite.Queries.GetFavorites;

/// <summary>
/// Query lấy danh sách cảm xúc mà người dùng đã thể hiện với các media.
/// </summary>
/// <param name="UserId">Mã người dùng hiện tại.</param>
public record GetFavoritesQuery(string UserId) : IRequest<List<FavoriteSummaryDto>>;
