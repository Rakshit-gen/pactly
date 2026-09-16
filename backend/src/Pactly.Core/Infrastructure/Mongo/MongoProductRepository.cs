using MongoDB.Driver;
using Pactly.Core.Domain;
using Pactly.Core.Repositories;

namespace Pactly.Core.Infrastructure.Mongo;

public class MongoProductRepository : IProductRepository
{
    private readonly MongoContext _context;

    public MongoProductRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetActiveAsync()
    {
        return await _context.Products.Find(p => p.IsActive).ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(string id)
    {
        return await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Product?> GetBySlugAsync(string slug)
    {
        return await _context.Products.Find(p => p.Slug == slug).FirstOrDefaultAsync();
    }
}
