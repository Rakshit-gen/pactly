using Pactly.Core.Domain;

namespace Pactly.Core.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(string userId);
    Task SaveAsync(Cart cart);
    Task ClearAsync(string userId);
}
