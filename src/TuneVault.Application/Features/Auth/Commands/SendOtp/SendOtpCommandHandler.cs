// Application/Features/Auth/Commands/SendOtp/SendOtpCommandHandler.cs
using MediatR;
using TuneVault.Application.Abstractions;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Auth.Commands.SendOtp;

public sealed class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, Unit>
{
    private readonly IOtpLogRepository _otpRepo;
    private readonly IEmailService _emailService;

    public SendOtpCommandHandler(
        IOtpLogRepository otpRepo,
        IEmailService emailService)
    {
        _otpRepo = otpRepo;
        _emailService = emailService;
    }

    public async Task<Unit> Handle(SendOtpCommand request, CancellationToken cancellationToken)
    {
        // BƯỚC 1: Validate purpose — chỉ chấp nhận 2 giá trị theo Rule 7
        if (request.Purpose != "register" && request.Purpose != "reset_password")
            throw new DomainException("Mục đích gửi OTP không hợp lệ. Chỉ chấp nhận 'register' hoặc 'reset_password'.");

        // BƯỚC 2: Sinh mã OTP 6 chữ số ngẫu nhiên (Rule 7: 6 chữ số)
        var otpCode  = GenerateOtpCode();
        var expiresAt = DateTime.UtcNow.AddMinutes(5); // Rule 7: hết hạn sau 5 phút

        // BƯỚC 3: Sinh Id cho bản ghi OtpLog và lưu vào DB
        var otpId = await _otpRepo.GenerateNextIdAsync(cancellationToken);
        await _otpRepo.InsertAsync(otpId, request.Email, otpCode, request.Purpose, expiresAt, cancellationToken);

        // BƯỚC 4: Gửi OTP qua IEmailService (Rule 8: không tự viết logic gửi email)
        // DevMode được xử lý bên trong GmailSmtpEmailService — Handler không cần biết
        await _emailService.SendOtpAsync(request.Email, otpCode, request.Purpose, cancellationToken);

        return Unit.Value;
    }

    /// <summary>Sinh mã OTP 6 chữ số ngẫu nhiên theo Rule 7.</summary>
    private static string GenerateOtpCode()
    {
        return Random.Shared.Next(0, 999999).ToString("D6");
    }
}