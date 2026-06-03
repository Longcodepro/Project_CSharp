using TuneVault.Domain.Enums;

namespace TuneVault.Application.Abstractions;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
}
