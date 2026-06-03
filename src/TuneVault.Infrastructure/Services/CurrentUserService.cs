using TuneVault.Application.Abstractions;
using TuneVault.Domain.Enums;

namespace TuneVault.Infrastructure.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    public Guid? UserId => null;
    public string? UserName => null;
    public UserRole? Role => null;
    public bool IsAuthenticated => false;
}
