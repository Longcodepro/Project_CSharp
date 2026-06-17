using MediatR;
using TuneVault.Application.Features.Auth.DTOs;
using TuneVault.Application.Interfaces;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepo;
    private readonly IAdminRepository _adminRepo;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUserRepository userRepo,
        IAdminRepository adminRepo,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepo = userRepo;
        _adminRepo = adminRepo;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var roles = new List<string>();
        string userId;
        string tokenUsername;
        string passwordHashInDb;

        // BƯỚC 1: Thử tìm Admin theo username (= email của admin)
        var admin = await _adminRepo.GetByUsernameAsync(request.IdDisplay, cancellationToken);

        if (admin != null)
        {
            userId          = admin.Id;
            tokenUsername   = admin.Email;
            passwordHashInDb = admin.PasswordHash;
            roles.Add("Admin");
            roles.Add(admin.Role);
        }
        else
        {
            // BƯỚC 2: Tìm User thường theo IdDisplay
            var user = await _userRepo.GetByIdDisplayAsync(request.IdDisplay, cancellationToken);

            if (user == null)
                throw new UnauthorizedAccessException("Tài khoản hoặc mật khẩu không chính xác.");

            userId          = user.Id;
            tokenUsername   = user.IdDisplay;
            passwordHashInDb = user.PasswordHash;

            roles.Add(user.IsArtist ? "Artist" : "Listener");
        }

        // BƯỚC 3: Xác minh mật khẩu bằng BCrypt
        if (!BCrypt.Net.BCrypt.Verify(request.Password, passwordHashInDb))
            throw new UnauthorizedAccessException("Tài khoản hoặc mật khẩu không chính xác.");

        // BƯỚC 4: Sinh JWT
        var token = _jwtTokenGenerator.GenerateToken(userId, tokenUsername, string.Join(",", roles));

        return new AuthResponseDto(
            AccessToken: token,
            UserId:      userId,
            IdDisplay:   tokenUsername,
            Roles:       roles
        );
    }
}
