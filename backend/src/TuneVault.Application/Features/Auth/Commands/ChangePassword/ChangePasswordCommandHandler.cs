using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpLogRepository _otpRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IOtpLogRepository otpRepository,
        ICurrentUserContext currentUserContext)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new UnauthorizedAccessException("Chưa xác thực. Vui lòng đăng nhập trước khi đổi mật khẩu.");

        var user = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (user is null)
            throw new DomainException("Không tìm thấy tài khoản hiện tại.");

        if (!string.Equals(user.Email, request.Email.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Email OTP không khớp với tài khoản hiện tại.");

        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Mật khẩu cũ không chính xác.");

        var isOtpValid = await _otpRepository.VerifyAndConsumeAsync(
            user.Email, request.OtpCode, "change_password", cancellationToken);

        if (!isOtpValid)
            throw new DomainException("Mã OTP không hợp lệ hoặc đã hết hạn.");

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.ChangePassword(newPasswordHash);

        await _userRepository.UpdatePasswordHashAsync(user.Id, newPasswordHash, cancellationToken);

        return Unit.Value;
    }
}
