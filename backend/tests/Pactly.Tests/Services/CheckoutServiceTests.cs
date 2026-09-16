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
        out InMemoryAgreementRepository agreements)
    {
        orders = new InMemoryOrderRepository();
        agreements = new InMemoryAgreementRepository();
        var agreementService = new AgreementService(agreements, orders, users, products);
        var prorationService = new ProrationService();

        return new CheckoutService(carts, products, orders, agreements, users, prorationService, agreementService);
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

        await Assert.ThrowsAsync<EmptyCartException>(() => service.CheckoutAsync("user-1"));
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

        var (order, agreement) = await service.CheckoutAsync("user-1");

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

        var (order, _) = await service.CheckoutAsync("user-1");

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

        var (order, _) = await service.CheckoutAsync("user-1");

        Assert.Equal(5900, order.SubtotalCents);
        Assert.True(order.ProrationCreditCents > 0);
        Assert.True(order.TotalCents < order.SubtotalCents);
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

        var (order, _) = await service.CheckoutAsync("user-1");

        Assert.Equal(0, order.ProrationCreditCents);
        Assert.Equal(order.SubtotalCents, order.TotalCents);
    }
}
