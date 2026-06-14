// Application/Features/Auth/Commands/SendOtp/SendOtpCommand.cs
using MediatR;

namespace TuneVault.Application.Features.Auth.Commands.SendOtp;

public sealed record SendOtpCommand(string Email, string Purpose) : IRequest<Unit>;