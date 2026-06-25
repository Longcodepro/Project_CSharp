using MediatR;
using TuneVault.Application.Features.CollectionLike.DTOs;

namespace TuneVault.Application.Features.CollectionLike.Queries.GetRecentCollectionLikes;

/// <summary>
/// Query lấy album/playlist người dùng thích gần nhất.
/// </summary>
/// <param name="Limit">Số lượng tối đa cần lấy.</param>
public sealed record GetRecentCollectionLikesQuery(int Limit = 3) : IRequest<IReadOnlyCollection<CollectionLikeDto>>;
