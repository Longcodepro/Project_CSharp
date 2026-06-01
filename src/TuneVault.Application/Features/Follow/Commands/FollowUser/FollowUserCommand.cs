using TuneVault.Application.DTOs.Follow;

namespace TuneVault.Application.Features.Follow.Commands.FollowUser;

public sealed record FollowUserCommand(string FollowerId, FollowUserRequestDto Request);
