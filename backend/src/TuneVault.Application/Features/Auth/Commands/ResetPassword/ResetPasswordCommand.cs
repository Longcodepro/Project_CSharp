// Application/Features/Auth/Commands/ResetPassword/ResetPasswordCommand.cs
using MediatR;

namespace TuneVault.Application.Features.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(string Email, string OtpCode, string NewPassword) : IRequest<Unit>;