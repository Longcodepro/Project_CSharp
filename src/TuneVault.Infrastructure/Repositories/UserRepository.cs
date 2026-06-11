using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    public Task AddAsync(User user, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<User?> GetByIdDisplayAsync(string idDisplay, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
