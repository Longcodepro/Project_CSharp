using MediatR;
using TuneVault.Application.Features.Auth.DTOs;
using TuneVault.Application.Interfaces;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Auth.Commands.Login;

/// <summary>
/// Xác thực đăng nhập và cấp token cho người dùng.
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    /// <summary>
    /// Khởi tạo handler đăng nhập.
    /// </summary>
    public LoginCommandHandler(
        IUserRepository userRepo,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepo = userRepo;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    /// <summary>
    /// Kiểm tra mật khẩu và trả về access token, refresh token.
    /// </summary>
    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdDisplayAsync(request.IdDisplay, cancellationToken);

        if (user == null)
            throw new UnauthorizedAccessException("Tài khoản hoặc mật khẩu không chính xác.");

        var roles = new List<string> { user.IsArtist ? "Artist" : "Listener" };
        var userId = user.Id;
        var tokenUsername = user.IdDisplay;
        var passwordHashInDb = user.PasswordHash;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, passwordHashInDb))
            throw new UnauthorizedAccessException("Tài khoản hoặc mật khẩu không chính xác.");

        var token = _jwtTokenGenerator.GenerateToken(userId, tokenUsername, string.Join(",", roles));
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(userId, tokenUsername, string.Join(",", roles));

        return new AuthResponseDto(
            AccessToken: token,
            RefreshToken: refreshToken,
            UserId:      userId,
            IdDisplay:   tokenUsername,
            Roles:       roles
        );
    }
}
