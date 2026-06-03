using TuneVault.Application.Abstractions;
using TuneVault.Domain.Enums;

namespace TuneVault.Infrastructure.Services;

public sealed class TokenService : ITokenService
{
    public string CreateToken(Guid userId, string email, UserRole role)
    {
        throw new NotImplementedException("Implement JWT creation here.");
    }
}
