namespace TuneVault.Application.Abstractions;

public interface ICurrentUserService
{
    string? UserId { get; }

    string? UserName { get; }

    bool IsAuthenticated { get; }
}
