using TuneVault.Application.Abstractions;

namespace TuneVault.Infrastructure.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    public string? UserId => throw new NotImplementedException();

    public string? UserName => throw new NotImplementedException();

    public bool IsAuthenticated => throw new NotImplementedException();
}
