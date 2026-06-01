namespace TuneVault.Application.Features.Follow.Commands.UnfollowUser;

public sealed record UnfollowUserCommand(string FollowerId, string FolloweeId);
