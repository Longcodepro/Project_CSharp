using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Xử lý đổi mật khẩu sau khi OTP hợp lệ.
/// </summary>
public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IUserRepository _userRepo;
    private readonly IOtpLogRepository _otpRepo;

    /// <summary>
    /// Khởi tạo handler đổi mật khẩu.
    /// </summary>
    public ResetPasswordCommandHandler(
        IUserRepository userRepo,
        IOtpLogRepository otpRepo)
    {
        _userRepo = userRepo;
        _otpRepo  = otpRepo;
    }

    /// <summary>
    /// Xác thực OTP và lưu mật khẩu mới.
    /// </summary>
    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var isOtpValid = await _otpRepo.VerifyAndConsumeAsync(
            request.Email, request.OtpCode, "reset_password", cancellationToken);

        if (!isOtpValid)
            throw new DomainException("Mã OTP không hợp lệ hoặc đã hết hạn.");

        var user = await _userRepo.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            throw new DomainException("Không tìm thấy người dùng với email này.");

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        user.ChangePassword(newPasswordHash);

        await _userRepo.UpdatePasswordHashAsync(user.Id, newPasswordHash, cancellationToken);

        return Unit.Value;
    }
}
