using TuneVault.Domain.Enums;

namespace TuneVault.Application.Abstractions;

public interface ITokenService
{
    string CreateToken(Guid userId, string email, UserRole role);
}
