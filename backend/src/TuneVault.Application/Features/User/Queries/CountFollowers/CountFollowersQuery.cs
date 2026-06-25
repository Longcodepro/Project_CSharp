using MediatR;

namespace TuneVault.Application.Features.User.Queries.CountFollowers;

/// <summary>
/// Query to count the number of followers for a specific user.
/// </summary>
/// <param name="UserId">The ID of the user whose followers are to be counted.</param>
public sealed record CountFollowersQuery(string UserId) : IRequest<int>;