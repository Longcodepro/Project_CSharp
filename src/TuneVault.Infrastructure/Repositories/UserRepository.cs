using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    public Task CreateAsync(User user, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
