using MediatR;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Favorite.Queries.CheckFavoriteStatus;

public sealed class CheckFavoriteStatusQueryHandler : IRequestHandler<CheckFavoriteStatusQuery, bool>
{
    private readonly IFavoriteRepository _favoriteRepository;

    public CheckFavoriteStatusQueryHandler(IFavoriteRepository favoriteRepository)
    {
        _favoriteRepository = favoriteRepository;
    }

    public async Task<bool> Handle(CheckFavoriteStatusQuery request, CancellationToken cancellationToken)
    {
        // Step 1: Use the repository to check if the media item is favorited by the user.
        var favorite = await _favoriteRepository.GetByUserIdAndMediaItemIdAsync(request.UserId, request.MediaItemId, cancellationToken);
        return favorite is not null;
    }
}
