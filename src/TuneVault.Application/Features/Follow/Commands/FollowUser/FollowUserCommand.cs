using TuneVault.Application.Features.Follow.DTOs;

namespace TuneVault.Application.Features.Follow.Commands.FollowUser;

public sealed record FollowUserCommand(string FollowerId, FollowUserRequestDto Request);
