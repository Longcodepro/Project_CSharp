using MediatR;
using TuneVault.Application.Abstractions; // For IEmailService
using TuneVault.Application.Features.Auth.DTOs;
using TuneVault.Application.Interfaces; // For IJwtTokenGenerator
using TuneVault.Domain.Entities;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces; // For IUserRepository

namespace TuneVault.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(string Email, string OtpCode, string IdDisplay, string DisplayName, string Password) : IRequest<AuthResponseDto>;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepo;
    private readonly IOtpLogRepository _otpRepo;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailService _emailService; // For sending OTP

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

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify OTP
        var isOtpValid = await _otpRepo.VerifyAndConsumeAsync(request.Email, request.OtpCode, "register", cancellationToken);
        if (!isOtpValid)
        {
            throw new DomainException("Mã OTP không hợp lệ hoặc đã hết hạn.");
        }

        // 2. Check if user already exists
        var existingUser = await _userRepo.GetByIdDisplayAsync(request.IdDisplay, cancellationToken);
        if (existingUser != null)
        {
            throw new DomainException($"Tên người dùng '{request.IdDisplay}' đã tồn tại.");
        }

        // 3. Hash password (using BCrypt)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 4. Create new user
        var newUserId = await _userRepo.GenerateNextIdAsync(cancellationToken);
        var newUser = new TuneVault.Domain.Entities.User(
            newUserId,
            request.IdDisplay,
            request.DisplayName,
            request.Email.ToLowerInvariant(),
            passwordHash
        );

        await _userRepo.InsertAsync(newUser, cancellationToken);

        // 5. Generate JWT Token
        var userRoles = new List<string> { "Listener" }; // Default role for new users
        var token = _jwtTokenGenerator.GenerateToken(newUser.Id, newUser.IdDisplay, userRoles);

        // 6. Return AuthResponseDto
        return new AuthResponseDto(
            AccessToken: token,
            UserId: newUser.Id,
            IdDisplay: newUser.IdDisplay,
            Roles: userRoles
        );
    }
}