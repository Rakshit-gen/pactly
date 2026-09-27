using Pactly.Core.Domain;

namespace Pactly.Core.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Inserts a new user. Returns false instead of throwing if a user with this email already
    /// exists (checked atomically at the storage layer, not via a separate check-then-insert).
    /// </summary>
    Task<bool> CreateAsync(User user);
    Task UpdateAsync(User user);
}
