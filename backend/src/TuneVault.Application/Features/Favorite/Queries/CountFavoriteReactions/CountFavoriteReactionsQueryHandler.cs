using MediatR;
using TuneVault.Application.Features.Favorite.DTOs;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Favorite.Queries.CountFavoriteReactions;

/// <summary>
/// Handler đếm tổng reaction đang được lưu trong Favorites.
/// </summary>
public sealed class CountFavoriteReactionsQueryHandler
    : IRequestHandler<CountFavoriteReactionsQuery, FavoriteReactionCountDto>
{
    private readonly IFavoriteRepository _favoriteRepository;

    public CountFavoriteReactionsQueryHandler(IFavoriteRepository favoriteRepository)
    {
        _favoriteRepository = favoriteRepository;
    }

    public async Task<FavoriteReactionCountDto> Handle(
        CountFavoriteReactionsQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TargetId))
        {
            throw new DomainException("Mã nội dung cần đếm cảm xúc là bắt buộc.");
        }

        var count = await _favoriteRepository.CountReactionsAsync(
            request.TargetId.Trim(),
            request.TargetType,
            cancellationToken);

        return new FavoriteReactionCountDto(request.TargetId.Trim(), request.TargetType, count);
    }
}
