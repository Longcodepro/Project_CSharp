using TuneVault.Application.DTOs.User;

namespace TuneVault.Application.Features.User.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(string UserId, UpdateProfileRequestDto Request);
