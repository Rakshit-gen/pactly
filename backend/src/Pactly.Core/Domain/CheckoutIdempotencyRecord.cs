using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pactly.Core.Domain;

public enum CheckoutIdempotencyStatus
{
    Pending,
    Completed
}

/// <summary>
/// One row per (UserId, Key) reserved by <see cref="Services.CheckoutService"/> so a retried or
/// double-fired checkout request replays the original result instead of creating a second order.
/// The Mongo repository enforces the (UserId, Key) uniqueness with a unique index; this type has
/// no invariants of its own beyond what that index guarantees.
/// </summary>
public class CheckoutIdempotencyRecord
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public CheckoutIdempotencyStatus Status { get; set; } = CheckoutIdempotencyStatus.Pending;
    public string? OrderId { get; set; }
    public string? AgreementId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
