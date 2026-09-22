using Pactly.Core.Domain;
using Pactly.Core.Services;
using Pactly.Tests.Fakes;
using Xunit;

namespace Pactly.Tests.Services;

public class CheckoutServiceTests
{
    private static Product Plan(string id, string slug, int monthlyPriceCents) => new()
    {
        Id = id,
        Slug = slug,
        Name = slug,
        Type = ProductType.Plan,
        MonthlyPriceCents = monthlyPriceCents,
        IsActive = true
    };

    private static Product AddOn(string id, string slug, int monthlyPriceCents) => new()
    {
        Id = id,
        Slug = slug,
        Name = slug,
        Type = ProductType.AddOn,
        MonthlyPriceCents = monthlyPriceCents,
        IsActive = true
    };

    private static CheckoutService BuildCheckoutService(
        InMemoryProductRepository products,
        InMemoryCartRepository carts,
        InMemoryUserRepository users,
        out InMemoryOrderRepository orders,
        out InMemoryAgreementRepository agreements,
        InMemoryCheckoutIdempotencyRepository? idempotencyKeys = null)
    {
        orders = new InMemoryOrderRepository();
        agreements = new InMemoryAgreementRepository();
        var agreementService = new AgreementService(agreements, orders, users, products);
        var prorationService = new ProrationService();

        return new CheckoutService(
            carts,
            products,
            orders,
            agreements,
            users,
            idempotencyKeys ?? new InMemoryCheckoutIdempotencyRepository(),
            prorationService,
            agreementService);
    }

    [Fact]
    public async Task CheckoutAsync_ThrowsWhenCartIsEmpty()
    {
        var users = new InMemoryUserRepository();
        await users.CreateAsync(new User { Id = "user-1", Email = "jane@example.com" });
        var service = BuildCheckoutService(
            new InMemoryProductRepository(),
            new InMemoryCartRepository(),
            users,
            out _,
            out _);

        await Assert.ThrowsAsync<EmptyCartException>(() => service.CheckoutAsync("user-1", "key-1"));
    }

    [Fact]
    public async Task CheckoutAsync_ThrowsWhenIdempotencyKeyIsMissing()
    {
        var users = new InMemoryUserRepository();
        await users.CreateAsync(new User { Id = "user-1", Email = "jane@example.com" });
        var service = BuildCheckoutService(
            new InMemoryProductRepository(),
            new InMemoryCartRepository(),
            users,
            out _,
            out _);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CheckoutAsync("user-1", ""));
    }

    [Fact]
    public async Task CheckoutAsync_RetriedWithSameKey_ReplaysOriginalResultInsteadOfCheckingOutAgain()
    {
        var products = new InMemoryProductRepository(new[] { Plan("plan-starter", "starter", 2900) });
        var carts = new InMemoryCartRepository();
        var users = new InMemoryUserRepository();
        await users.CreateAsync(new User { Id = "user-1", Email = "jane@example.com" });
        var cartService = new CartService(carts, products);
        await cartService.AddToCartAsync("user-1", "plan-starter", 1);

        var idempotencyKeys = new InMemoryCheckoutIdempotencyRepository();
        var service = BuildCheckoutService(
            products, carts, users, out var orders, out _, idempotencyKeys);

        var (firstOrder, firstAgreement) = await service.CheckoutAsync("user-1", "retry-key");
        // A second attempt with the same key must not touch an empty cart and fail; it should
        // short-circuit to the first attempt's result before ever reaching the cart check.
        var (secondOrder, secondAgreement) = await service.CheckoutAsync("user-1", "retry-key");

        Assert.Equal(firstOrder.Id, secondOrder.Id);
        Assert.Equal(firstAgreement.Id, secondAgreement.Id);
        Assert.Single(await orders.GetByUserIdAsync("user-1"));
    }

    [Fact]
    public async Task CheckoutAsync_SecondCallWithSameKeyWhileFirstStillPending_ThrowsInProgress()
    {
        var products = new InMemoryProductRepository(new[] { Plan("plan-starter", "starter", 2900) });
        var carts = new InMemoryCartRepository();
        var users = new InMemoryUserRepository();
        await users.CreateAsync(new User { Id = "user-1", Email = "jane@example.com" });
        var cartService = new CartService(carts, products);
        await cartService.AddToCartAsync("user-1", "plan-starter", 1);

        var idempotencyKeys = new InMemoryCheckoutIdempotencyRepository();
        // Simulate a first attempt that reserved the key but hasn't completed yet (e.g. a
        // concurrent request still mid-flight).
        await idempotencyKeys.TryReserveAsync("user-1", "in-flight-key");

        var service = BuildCheckoutService(
            products, carts, users, out _, out _, idempotencyKeys);

        await Assert.ThrowsAsync<CheckoutInProgressException>(
            () => service.CheckoutAsync("user-1", "in-flight-key"));
    }

    [Fact]
    public async Task CheckoutAsync_KeyIsReleasedAfterAFailedAttempt_SoARealRetryCanSucceed()
    {
        var products = new InMemoryProductRepository(new[] { Plan("plan-starter", "starter", 2900) });
        var carts = new InMemoryCartRepository();
        var users = new InMemoryUserRepository();
        await users.CreateAsync(new User { Id = "user-1", Email = "jane@example.com" });

        var idempotencyKeys = new InMemoryCheckoutIdempotencyRepository();
        var service = BuildCheckoutService(
            products, carts, users, out _, out _, idempotencyKeys);

        // Cart is empty, so this attempt fails and must release its reservation.
        await Assert.ThrowsAsync<EmptyCartException>(() => service.CheckoutAsync("user-1", "reused-key"));

        var cartService = new CartService(carts, products);
        await cartService.AddToCartAsync("user-1", "plan-starter", 1);

        // Same key, now with items in the cart: this must succeed, not throw CheckoutInProgress.
        var (order, _) = await service.CheckoutAsync("user-1", "reused-key");
        Assert.Equal(2900, order.SubtotalCents);
    }

    [Fact]
    public async Task CheckoutAsync_FreshPurchase_ChargesFullSubtotalAndSendsAgreementForSignature()
    {
        var products = new InMemoryProductRepository(new[] { Plan("plan-starter", "starter", 2900) });
        var carts = new InMemoryCartRepository();
        var users = new InMemoryUserRepository();
        await users.CreateAsync(new User { Id = "user-1", Email = "jane@example.com", CurrentSeats = 0 });
        var cartService = new CartService(carts, products);
        await cartService.AddToCartAsync("user-1", "plan-starter", 4);

        var service = BuildCheckoutService(products, carts, users, out var orders, out var agreements);

        var (order, agreement) = await service.CheckoutAsync("user-1", "key-1");

        Assert.Equal(11600, order.SubtotalCents);
        Assert.Equal(0, order.ProrationCreditCents);
        Assert.Equal(11600, order.TotalCents);
        Assert.Equal(AgreementStatus.AwaitingSignature, agreement.Status);
        Assert.Null(await carts.GetByUserIdAsync("user-1"));
    }

    [Fact]
    public async Task CheckoutAsync_IncludesAddOnPricingInSubtotal()
    {
        var products = new InMemoryProductRepository(new[]
        {
            Plan("plan-starter", "starter", 2900),
            AddOn("addon-storage", "extra-storage", 500)
        });
        var carts = new InMemoryCartRepository();
        var users = new InMemoryUserRepository();
        await users.CreateAsync(new User { Id = "user-1", Email = "jane@example.com" });
        var cartService = new CartService(carts, products);
        await cartService.AddToCartAsync("user-1", "plan-starter", 1);
        await cartService.AddToCartAsync("user-1", "addon-storage", 2);

        var service = BuildCheckoutService(products, carts, users, out _, out _);

        var (order, _) = await service.CheckoutAsync("user-1", "key-1");

        Assert.Equal(2900 + (500 * 2), order.SubtotalCents);
    }

    [Fact]
    public async Task CheckoutAsync_MidCycleUpgrade_AppliesProrationCredit()
    {
        var products = new InMemoryProductRepository(new[]
        {
            Plan("plan-starter", "starter", 2900),
            Plan("plan-growth", "growth", 5900)
        });
        var carts = new InMemoryCartRepository();
        var users = new InMemoryUserRepository();

        var periodStart = DateTimeOffset.UtcNow.AddDays(-15);
        var periodEnd = DateTimeOffset.UtcNow.AddDays(15);
        await users.CreateAsync(new User
        {
            Id = "user-1",
            Email = "jane@example.com",
            CurrentPlanId = "plan-starter",
            CurrentSeats = 1,
            CurrentPeriodStart = periodStart,
            CurrentPeriodEnd = periodEnd
        });

        var cartService = new CartService(carts, products);
        await cartService.AddToCartAsync("user-1", "plan-growth", 1);

        var service = BuildCheckoutService(products, carts, users, out _, out _);

        var (order, _) = await service.CheckoutAsync("user-1", "key-1");

        Assert.Equal(5900, order.SubtotalCents);
        Assert.True(order.ProrationCreditCents > 0);
        Assert.True(order.TotalCents < order.SubtotalCents);
        // Half the period left: charge 2950 for the new plan, minus 1450 credit for the old one.
        Assert.InRange(order.TotalCents, 1400, 1600);
    }

    [Fact]
    public async Task CheckoutAsync_SameplanSameSeats_DoesNotApplyProration()
    {
        var products = new InMemoryProductRepository(new[] { Plan("plan-starter", "starter", 2900) });
        var carts = new InMemoryCartRepository();
        var users = new InMemoryUserRepository();

        await users.CreateAsync(new User
        {
            Id = "user-1",
            Email = "jane@example.com",
            CurrentPlanId = "plan-starter",
            CurrentSeats = 3,
            CurrentPeriodStart = DateTimeOffset.UtcNow.AddDays(-10),
            CurrentPeriodEnd = DateTimeOffset.UtcNow.AddDays(20)
        });

        var cartService = new CartService(carts, products);
        await cartService.AddToCartAsync("user-1", "plan-starter", 3);

        var service = BuildCheckoutService(products, carts, users, out _, out _);

        var (order, _) = await service.CheckoutAsync("user-1", "key-1");

        Assert.Equal(0, order.ProrationCreditCents);
        Assert.Equal(order.SubtotalCents, order.TotalCents);
    }
}
