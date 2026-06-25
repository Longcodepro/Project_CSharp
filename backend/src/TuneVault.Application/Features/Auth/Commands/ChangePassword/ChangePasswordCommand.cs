using MediatR;

namespace TuneVault.Application.Features.Auth.Commands.ChangePassword;

public sealed record ChangePasswordCommand(string Email, string OldPassword, string OtpCode, string NewPassword) : IRequest<Unit>;
