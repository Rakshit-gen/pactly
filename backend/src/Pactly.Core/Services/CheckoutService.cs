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

public class CheckoutService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IAgreementRepository _agreementRepository;
    private readonly IUserRepository _userRepository;
    private readonly ProrationService _prorationService;
    private readonly AgreementService _agreementService;

    public CheckoutService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IAgreementRepository agreementRepository,
        IUserRepository userRepository,
        ProrationService prorationService,
        AgreementService agreementService)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _agreementRepository = agreementRepository;
        _userRepository = userRepository;
        _prorationService = prorationService;
        _agreementService = agreementService;
    }

    public async Task<(Order Order, Agreement Agreement)> CheckoutAsync(string userId)
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
            totalCents = addOnTotalCents + proration.ChargeCents;
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
