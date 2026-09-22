using Pactly.Core.Domain;

namespace Pactly.Core.Repositories;

public interface ICheckoutIdempotencyRepository
{
    /// <summary>
    /// Atomically claims (userId, key) for a new checkout attempt. Returns false, without
    /// throwing, if that key is already reserved — by this attempt racing itself or by a prior
    /// one — so the caller can decide whether to replay a completed result or reject as in-progress.
    /// </summary>
    Task<bool> TryReserveAsync(string userId, string key);

    Task<CheckoutIdempotencyRecord?> FindAsync(string userId, string key);

    Task CompleteAsync(string userId, string key, string orderId, string agreementId);

    /// <summary>Releases a reservation after a failed checkout attempt so the same key can be retried.</summary>
    Task ReleaseAsync(string userId, string key);
}
