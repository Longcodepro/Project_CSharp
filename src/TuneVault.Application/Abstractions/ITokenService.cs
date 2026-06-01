namespace TuneVault.Application.Abstractions;

public interface ITokenService
{
    Task<string> CreateTokenAsync(string userId, string userName, string role, CancellationToken cancellationToken = default);
}
