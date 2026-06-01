using TuneVault.Application.Abstractions;

namespace TuneVault.Infrastructure.Services;

public sealed class TokenService : ITokenService
{
    public Task<string> CreateTokenAsync(string userId, string userName, string role, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
