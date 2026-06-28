using MediatR;
using TuneVault.Application.Abstractions;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Auth.Commands.SendOtp;

/// <summary>
/// Sinh OTP và gửi qua email cho người dùng.
/// </summary>
public sealed class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, Unit>
{
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromSeconds(90);

    private readonly IOtpLogRepository _otpRepo;
    private readonly IEmailService _emailService;

    /// <summary>
    /// Khởi tạo handler gửi OTP.
    /// </summary>
    public SendOtpCommandHandler(
        IOtpLogRepository otpRepo,
        IEmailService emailService)
    {
        _otpRepo = otpRepo;
        _emailService = emailService;
    }

    /// <summary>
    /// Lưu OTP mới và gửi mã xác nhận qua email.
    /// </summary>
    public async Task<Unit> Handle(SendOtpCommand request, CancellationToken cancellationToken)
    {
        if (request.Purpose != "register" && request.Purpose != "reset_password" && request.Purpose != "change_password")
            throw new DomainException("Mục đích gửi OTP không hợp lệ. Chỉ chấp nhận 'register', 'reset_password' hoặc 'change_password'.");

        var otpCode  = GenerateOtpCode();
        var expiresAt = DateTime.UtcNow.Add(OtpLifetime);

        var otpId = await _otpRepo.GenerateNextIdAsync(cancellationToken);
        await _otpRepo.InsertAsync(otpId, request.Email, otpCode, request.Purpose, expiresAt, cancellationToken);

        await _emailService.SendOtpAsync(request.Email, otpCode, request.Purpose, cancellationToken);

        return Unit.Value;
    }

    /// <summary>Sinh mã OTP 6 chữ số ngẫu nhiên.</summary>
    private static string GenerateOtpCode()
    {
        return Random.Shared.Next(0, 999999).ToString("D6");
    }
}
