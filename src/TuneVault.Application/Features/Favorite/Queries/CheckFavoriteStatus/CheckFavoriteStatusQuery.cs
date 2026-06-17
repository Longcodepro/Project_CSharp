using MediatR;

namespace TuneVault.Application.Features.Favorite.Queries.CheckFavoriteStatus;

public record CheckFavoriteStatusQuery(string UserId, string MediaItemId) : IRequest<bool>;