namespace TuneVault.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(string userId, string email, string role);

    string GenerateRefreshToken(string userId, string email, string role);
}
