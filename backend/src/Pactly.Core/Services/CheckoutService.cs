using MongoDB.Bson;
using Pactly.Core.Domain;
using Pactly.Core.Repositories;

namespace Pactly.Core.Services;

public class EmptyCartException : Exception
{
    public EmptyCartException(string userId)
        : base($"Cart for user '{userId}' is empty, nothing to check out.")
    {
    }
}

/// <summary>
/// Thrown when a checkout is retried with an idempotency key that is still being processed by
/// another in-flight attempt (rather than one that already completed, which replays instead).
/// A client should back off and retry, not treat this as a hard failure.
/// </summary>
public class CheckoutInProgressException : Exception
{
    public CheckoutInProgressException(string idempotencyKey)
        : base($"A checkout for idempotency key '{idempotencyKey}' is already in progress.")
    {
    }
}

public class CheckoutService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IAgreementRepository _agreementRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICheckoutIdempotencyRepository _idempotencyRepository;
    private readonly ProrationService _prorationService;
    private readonly AgreementService _agreementService;

    public CheckoutService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IAgreementRepository agreementRepository,
        IUserRepository userRepository,
        ICheckoutIdempotencyRepository idempotencyRepository,
        ProrationService prorationService,
        AgreementService agreementService)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _agreementRepository = agreementRepository;
        _userRepository = userRepository;
        _idempotencyRepository = idempotencyRepository;
        _prorationService = prorationService;
        _agreementService = agreementService;
    }

    /// <summary>
    /// Turns the caller's cart into an order and a drafted agreement. <paramref name="idempotencyKey"/>
    /// makes this safe to retry: a client-generated key that's replayed (network retry, double
    /// click that beat the frontend guard, two tabs) returns the original order and agreement
    /// instead of checking out twice. A key still in flight raises <see cref="CheckoutInProgressException"/>.
    /// </summary>
    public async Task<(Order Order, Agreement Agreement)> CheckoutAsync(string userId, string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("An idempotency key is required to check out.", nameof(idempotencyKey));
        }

        if (!await _idempotencyRepository.TryReserveAsync(userId, idempotencyKey))
        {
            var existing = await _idempotencyRepository.FindAsync(userId, idempotencyKey);
            if (existing?.Status == CheckoutIdempotencyStatus.Completed)
            {
                var existingOrder = await _orderRepository.GetByIdAsync(existing.OrderId!)
                    ?? throw new InvalidOperationException(
                        $"Idempotency record for key '{idempotencyKey}' points at a missing order '{existing.OrderId}'.");
                var existingAgreement = await _agreementRepository.GetByIdAsync(existing.AgreementId!)
                    ?? throw new InvalidOperationException(
                        $"Idempotency record for key '{idempotencyKey}' points at a missing agreement '{existing.AgreementId}'.");
                return (existingOrder, existingAgreement);
            }

            // Reserved but not yet completed: a concurrent attempt is mid-flight (or a prior one
            // crashed before releasing). Either way, running checkout again here would race it.
            throw new CheckoutInProgressException(idempotencyKey);
        }

        try
        {
            var (order, agreement) = await ExecuteCheckoutAsync(userId);
            await _idempotencyRepository.CompleteAsync(userId, idempotencyKey, order.Id, agreement.Id);
            return (order, agreement);
        }
        catch
        {
            // Don't leave a failed attempt's key permanently stuck in Pending — release it so a
            // genuine retry (not a duplicate of a successful checkout) can proceed.
            await _idempotencyRepository.ReleaseAsync(userId, idempotencyKey);
            throw;
        }
    }

    private async Task<(Order Order, Agreement Agreement)> ExecuteCheckoutAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User '{userId}' was not found.");

        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is null || cart.Lines.Count == 0)
        {
            throw new EmptyCartException(userId);
        }

        var orderLines = new List<OrderLine>();
        Product? planProduct = null;
        var planQuantity = 0;

        foreach (var line in cart.Lines)
        {
            var product = await _productRepository.GetByIdAsync(line.ProductId)
                ?? throw new ProductUnavailableException(line.ProductId);

            orderLines.Add(new OrderLine
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPriceCents = product.MonthlyPriceCents,
                Quantity = line.Quantity
            });

            if (product.Type == ProductType.Plan)
            {
                planProduct = product;
                planQuantity = line.Quantity;
            }
        }

        var subtotalCents = orderLines.Sum(l => l.UnitPriceCents * l.Quantity);
        var prorationCreditCents = 0;
        var totalCents = subtotalCents;

        var isMidCycleSwap = planProduct is not null
            && user.CurrentPlanId is not null
            && user.CurrentPeriodStart is not null
            && user.CurrentPeriodEnd is not null
            && user.CurrentPeriodEnd > DateTimeOffset.UtcNow
            && (user.CurrentPlanId != planProduct.Id || user.CurrentSeats != planQuantity);

        if (isMidCycleSwap)
        {
            var oldPlan = await _productRepository.GetByIdAsync(user.CurrentPlanId!);
            var oldMonthlyTotal = (oldPlan?.MonthlyPriceCents ?? 0) * user.CurrentSeats;
            var newMonthlyTotal = planProduct!.MonthlyPriceCents * planQuantity;

            var proration = _prorationService.Calculate(
                oldMonthlyTotal,
                newMonthlyTotal,
                user.CurrentPeriodStart!.Value,
                user.CurrentPeriodEnd!.Value,
                DateTimeOffset.UtcNow);

            prorationCreditCents = proration.CreditCents;

            var planLine = orderLines.First(l => l.ProductId == planProduct.Id);
            var addOnTotalCents = subtotalCents - (planLine.UnitPriceCents * planLine.Quantity);
            totalCents = addOnTotalCents + proration.NetCents;
        }

        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            Lines = orderLines,
            SubtotalCents = subtotalCents,
            ProrationCreditCents = prorationCreditCents,
            TotalCents = Math.Max(totalCents, 0),
            Status = OrderStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _orderRepository.CreateAsync(order);

        var agreement = new Agreement
        {
            Id = ObjectId.GenerateNewId().ToString(),
            OrderId = order.Id,
            UserId = userId,
            Status = AgreementStatus.Draft,
            ContentSnapshot = BuildContentSnapshot(user, order),
            AuditTrail = new List<AuditEntry>
            {
                new()
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    Actor = "system",
                    Action = "Created",
                    Metadata = $"Agreement drafted for order {order.Id}."
                }
            }
        };

        await _agreementRepository.CreateAsync(agreement);
        await _cartRepository.ClearAsync(userId);

        var sentAgreement = await _agreementService.RequestSignatureAsync(agreement.Id);

        return (order, sentAgreement);
    }

    private static string BuildContentSnapshot(User user, Order order)
    {
        var lines = string.Join(
            "\n",
            order.Lines.Select(l => $"- {l.ProductName} x{l.Quantity} at {FormatCents(l.UnitPriceCents)} per month"));

        return $"Agreement for {user.DisplayName} ({user.Email})\n\n{lines}\n\nTotal due today: {FormatCents(order.TotalCents)}";
    }

    private static string FormatCents(int cents) => $"${cents / 100.0:0.00}";
}
