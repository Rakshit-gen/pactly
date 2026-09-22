using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Pactly.Core.Domain;

namespace Pactly.Core.Infrastructure.Mongo;

public class MongoContext
{
    public IMongoCollection<Product> Products { get; }
    public IMongoCollection<User> Users { get; }
    public IMongoCollection<Cart> Carts { get; }
    public IMongoCollection<Order> Orders { get; }
    public IMongoCollection<Agreement> Agreements { get; }
    public IMongoCollection<CheckoutIdempotencyRecord> CheckoutIdempotencyKeys { get; }

    public MongoContext(IOptions<MongoOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        var database = client.GetDatabase(options.Value.DatabaseName);

        Products = database.GetCollection<Product>("products");
        Users = database.GetCollection<User>("users");
        Carts = database.GetCollection<Cart>("carts");
        Orders = database.GetCollection<Order>("orders");
        Agreements = database.GetCollection<Agreement>("agreements");
        CheckoutIdempotencyKeys = database.GetCollection<CheckoutIdempotencyRecord>("checkout_idempotency_keys");

        // Enforces at-most-one reservation per (user, key) even under concurrent requests;
        // MongoCheckoutIdempotencyRepository.TryReserveAsync relies on this to detect a race
        // by catching the duplicate-key write error rather than a separate check-then-insert.
        var indexKeys = Builders<CheckoutIdempotencyRecord>.IndexKeys
            .Ascending(r => r.UserId)
            .Ascending(r => r.Key);
        CheckoutIdempotencyKeys.Indexes.CreateOne(
            new CreateIndexModel<CheckoutIdempotencyRecord>(indexKeys, new CreateIndexOptions { Unique = true }));
    }
}
