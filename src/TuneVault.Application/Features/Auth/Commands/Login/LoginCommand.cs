using TuneVault.Application.DTOs.Auth;

namespace TuneVault.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(LoginRequestDto Request);
