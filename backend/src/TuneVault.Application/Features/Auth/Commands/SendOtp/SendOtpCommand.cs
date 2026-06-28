using MediatR;

namespace TuneVault.Application.Features.Auth.Commands.SendOtp;

/// <summary>
/// Yêu cầu gửi OTP cho đăng ký hoặc đổi mật khẩu.
/// </summary>
public sealed record SendOtpCommand(string Email, string Purpose) : IRequest<Unit>;
