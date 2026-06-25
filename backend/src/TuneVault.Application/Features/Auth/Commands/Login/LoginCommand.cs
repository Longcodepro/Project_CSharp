using MediatR;
using TuneVault.Application.Features.Auth.DTOs;

namespace TuneVault.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string IdDisplay, string Password) : IRequest<AuthResponseDto>;