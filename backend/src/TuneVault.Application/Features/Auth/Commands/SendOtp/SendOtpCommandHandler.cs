// Application/Features/Auth/Commands/SendOtp/SendOtpCommandHandler.cs
using MediatR;
using TuneVault.Application.Abstractions;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Auth.Commands.SendOtp;

public sealed class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, Unit>
{
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromSeconds(90);

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
        // BƯỚC 1: Validate purpose — chỉ chấp nhận các luồng OTP đã được thiết kế
        if (request.Purpose != "register" && request.Purpose != "reset_password" && request.Purpose != "change_password")
            throw new DomainException("Mục đích gửi OTP không hợp lệ. Chỉ chấp nhận 'register', 'reset_password' hoặc 'change_password'.");

        // BƯỚC 2: Sinh mã OTP 6 chữ số ngẫu nhiên
        var otpCode  = GenerateOtpCode();
        var expiresAt = DateTime.UtcNow.Add(OtpLifetime);

        // BƯỚC 3: Sinh Id cho bản ghi OtpLog và lưu vào DB
        var otpId = await _otpRepo.GenerateNextIdAsync(cancellationToken);
        await _otpRepo.InsertAsync(otpId, request.Email, otpCode, request.Purpose, expiresAt, cancellationToken);

        // BƯỚC 4: Gửi OTP qua IEmailService
        // DevMode được xử lý bên trong GmailSmtpEmailService — Handler không cần biết
        await _emailService.SendOtpAsync(request.Email, otpCode, request.Purpose, cancellationToken);

        return Unit.Value;
    }

    /// <summary>Sinh mã OTP 6 chữ số ngẫu nhiên.</summary>
    private static string GenerateOtpCode()
    {
        return Random.Shared.Next(0, 999999).ToString("D6");
    }
}
