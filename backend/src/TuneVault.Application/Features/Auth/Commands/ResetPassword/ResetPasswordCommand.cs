using MediatR;

namespace TuneVault.Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Yêu cầu đặt lại mật khẩu bằng OTP.
/// </summary>
public sealed record ResetPasswordCommand(string Email, string OtpCode, string NewPassword) : IRequest<Unit>;
