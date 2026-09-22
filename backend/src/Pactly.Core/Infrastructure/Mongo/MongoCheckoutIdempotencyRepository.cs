using MongoDB.Bson;
using MongoDB.Driver;
using Pactly.Core.Domain;
using Pactly.Core.Repositories;

namespace Pactly.Core.Infrastructure.Mongo;

public class MongoCheckoutIdempotencyRepository : ICheckoutIdempotencyRepository
{
    private readonly MongoContext _context;

    public MongoCheckoutIdempotencyRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task<bool> TryReserveAsync(string userId, string key)
    {
        var record = new CheckoutIdempotencyRecord
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            Key = key,
            Status = CheckoutIdempotencyStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        try
        {
            await _context.CheckoutIdempotencyKeys.InsertOneAsync(record);
            return true;
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            // (UserId, Key) is already reserved by this attempt or a prior one.
            return false;
        }
    }

    public async Task<CheckoutIdempotencyRecord?> FindAsync(string userId, string key)
    {
        return await _context.CheckoutIdempotencyKeys
            .Find(r => r.UserId == userId && r.Key == key)
            .FirstOrDefaultAsync();
    }

    public async Task CompleteAsync(string userId, string key, string orderId, string agreementId)
    {
        var update = Builders<CheckoutIdempotencyRecord>.Update
            .Set(r => r.Status, CheckoutIdempotencyStatus.Completed)
            .Set(r => r.OrderId, orderId)
            .Set(r => r.AgreementId, agreementId);

        await _context.CheckoutIdempotencyKeys.UpdateOneAsync(
            r => r.UserId == userId && r.Key == key,
            update);
    }

    public async Task ReleaseAsync(string userId, string key)
    {
        await _context.CheckoutIdempotencyKeys.DeleteOneAsync(r => r.UserId == userId && r.Key == key);
    }
}
