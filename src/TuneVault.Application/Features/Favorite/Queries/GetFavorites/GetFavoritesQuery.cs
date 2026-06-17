using MediatR;
using TuneVault.Application.Features.Media.DTOs;

namespace TuneVault.Application.Features.Favorite.Queries.GetFavorites;

public record GetFavoritesQuery(string UserId) : IRequest<List<MediaItemDto>>;