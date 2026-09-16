using MongoDB.Driver;
using Pactly.Core.Domain;
using Pactly.Core.Repositories;

namespace Pactly.Core.Infrastructure.Mongo;

public class MongoOrderRepository : IOrderRepository
{
    private readonly MongoContext _context;

    public MongoOrderRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(string id)
    {
        return await _context.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Order>> GetByUserIdAsync(string userId)
    {
        return await _context.Orders.Find(o => o.UserId == userId)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task CreateAsync(Order order)
    {
        await _context.Orders.InsertOneAsync(order);
    }

    public async Task UpdateAsync(Order order)
    {
        await _context.Orders.ReplaceOneAsync(o => o.Id == order.Id, order);
    }
}
