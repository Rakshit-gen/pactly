using MongoDB.Driver;
using Pactly.Core.Domain;
using Pactly.Core.Repositories;

namespace Pactly.Core.Infrastructure.Mongo;

public class MongoAgreementRepository : IAgreementRepository
{
    private readonly MongoContext _context;

    public MongoAgreementRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task<Agreement?> GetByIdAsync(string id)
    {
        return await _context.Agreements.Find(a => a.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Agreement>> GetByUserIdAsync(string userId)
    {
        return await _context.Agreements.Find(a => a.UserId == userId).ToListAsync();
    }

    public async Task CreateAsync(Agreement agreement)
    {
        await _context.Agreements.InsertOneAsync(agreement);
    }

    public async Task UpdateAsync(Agreement agreement)
    {
        await _context.Agreements.ReplaceOneAsync(a => a.Id == agreement.Id, agreement);
    }
}
