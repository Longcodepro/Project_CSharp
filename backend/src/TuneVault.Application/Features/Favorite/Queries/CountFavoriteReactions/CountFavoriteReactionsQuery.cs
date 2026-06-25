using MediatR;
using TuneVault.Application.Features.Favorite.DTOs;
using TuneVault.Domain.Enums;

namespace TuneVault.Application.Features.Favorite.Queries.CountFavoriteReactions;

/// <summary>
/// Query đếm tổng số lượt thể hiện cảm xúc của một media, album hoặc playlist.
/// </summary>
public sealed record CountFavoriteReactionsQuery(
    string TargetId,
    FavoriteTargetType TargetType) : IRequest<FavoriteReactionCountDto>;
