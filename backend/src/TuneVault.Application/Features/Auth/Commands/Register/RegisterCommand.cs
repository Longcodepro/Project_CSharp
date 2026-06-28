using MediatR;
using TuneVault.Application.Abstractions;
using TuneVault.Application.Features.Auth.DTOs;
using TuneVault.Application.Interfaces;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Auth.Commands.Register;

/// <summary>
/// Command đăng ký tài khoản người dùng mới sau khi email đã được xác minh bằng OTP.
/// </summary>
public sealed record RegisterCommand(string Email, string OtpCode, string IdDisplay, string DisplayName, string Password) : IRequest<AuthResponseDto>;

/// <summary>
/// Tạo tài khoản mới sau khi email đã xác minh OTP.
/// </summary>
public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepo;
    private readonly IOtpLogRepository _otpRepo;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailService _emailService;

    /// <summary>
    /// Khởi tạo handler đăng ký.
    /// </summary>
    public RegisterCommandHandler(
        IUserRepository userRepo,
        IOtpLogRepository otpRepo,
        IJwtTokenGenerator jwtTokenGenerator,
        IEmailService emailService)
    {
        _userRepo = userRepo;
        _otpRepo = otpRepo;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailService = emailService;
    }

    /// <summary>
    /// Kiểm tra OTP, tạo user và cấp token đăng nhập.
    /// </summary>
    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var isOtpValid = await _otpRepo.VerifyAndConsumeAsync(request.Email, request.OtpCode, "register", cancellationToken);
        if (!isOtpValid)
        {
            throw new DomainException("Mã OTP không hợp lệ hoặc đã hết hạn.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingEmail = await _userRepo.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingEmail != null)
        {
            throw new DomainException("Email này đã được sử dụng.");
        }

        var existingUser = await _userRepo.GetByIdDisplayAsync(request.IdDisplay, cancellationToken);
        if (existingUser != null)
        {
            throw new DomainException($"Tên người dùng '{request.IdDisplay}' đã tồn tại.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var newUserId = await _userRepo.GenerateNextIdAsync(cancellationToken);
        var newUser = new TuneVault.Domain.Entities.User(
            newUserId,
            request.IdDisplay,
            request.DisplayName,
            normalizedEmail,
            passwordHash
        );

        await _userRepo.InsertAsync(newUser, cancellationToken);

        var userRoles = new List<string> { "Listener" };
        var token = _jwtTokenGenerator.GenerateToken(newUser.Id, newUser.IdDisplay, string.Join(",", userRoles));
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(newUser.Id, newUser.IdDisplay, string.Join(",", userRoles));

        return new AuthResponseDto(
            AccessToken: token,
            RefreshToken: refreshToken,
            UserId: newUser.Id,
            IdDisplay: newUser.IdDisplay,
            Roles: userRoles
        );
    }
}
