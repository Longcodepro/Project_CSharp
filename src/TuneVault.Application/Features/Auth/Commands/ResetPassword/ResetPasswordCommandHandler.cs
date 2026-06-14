// Application/Features/Auth/Commands/ResetPassword/ResetPasswordCommandHandler.cs
using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IUserRepository _userRepo;
    private readonly IOtpLogRepository _otpRepo;

    public ResetPasswordCommandHandler(
        IUserRepository userRepo,
        IOtpLogRepository otpRepo)
    {
        _userRepo = userRepo;
        _otpRepo  = otpRepo;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // BƯỚC 1: Verify OTP — purpose = "reset_password" theo Rule 7
        var isOtpValid = await _otpRepo.VerifyAndConsumeAsync(
            request.Email, request.OtpCode, "reset_password", cancellationToken);

        if (!isOtpValid)
            throw new DomainException("Mã OTP không hợp lệ hoặc đã hết hạn.");

        // BƯỚC 2: Tìm user theo email
        var user = await _userRepo.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            throw new DomainException("Không tìm thấy người dùng với email này.");

        // BƯỚC 3: Hash mật khẩu mới bằng BCrypt
        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        // BƯỚC 4: Gọi method nghiệp vụ của entity để đổi mật khẩu (Rule 6: logic trong Domain)
        user.ChangePassword(newPasswordHash);

        // BƯỚC 5: Persist — dùng UpdatePasswordHashAsync để tối ưu, chỉ cập nhật đúng 1 cột
        await _userRepo.UpdatePasswordHashAsync(user.Id, newPasswordHash, cancellationToken);

        return Unit.Value;
    }
}