using MongoDB.Driver;
using Pactly.Core.Domain;
using Pactly.Core.Repositories;

namespace Pactly.Core.Infrastructure.Mongo;

public class MongoCartRepository : ICartRepository
{
    private readonly MongoContext _context;

    public MongoCartRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByUserIdAsync(string userId)
    {
        return await _context.Carts.Find(c => c.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task SaveAsync(Cart cart)
    {
        await _context.Carts.ReplaceOneAsync(
            c => c.UserId == cart.UserId,
            cart,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task ClearAsync(string userId)
    {
        await _context.Carts.DeleteOneAsync(c => c.UserId == userId);
    }
}
