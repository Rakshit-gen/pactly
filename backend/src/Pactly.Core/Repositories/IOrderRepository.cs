using Pactly.Core.Domain;

namespace Pactly.Core.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(string id);
    Task<List<Order>> GetByUserIdAsync(string userId);
    Task CreateAsync(Order order);
    Task UpdateAsync(Order order);
}
