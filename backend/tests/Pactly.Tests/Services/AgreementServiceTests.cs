using Pactly.Core.Domain;
using Pactly.Core.Services;
using Pactly.Tests.Fakes;
using Xunit;

namespace Pactly.Tests.Services;

public class AgreementServiceTests
{
    private static (AgreementService Service, InMemoryAgreementRepository Agreements, InMemoryOrderRepository Orders, InMemoryUserRepository Users) Build()
    {
        var agreements = new InMemoryAgreementRepository();
        var orders = new InMemoryOrderRepository();
        var users = new InMemoryUserRepository();
        var products = new InMemoryProductRepository(new[]
        {
            new Product
            {
                Id = "plan-growth",
                Slug = "growth",
                Name = "Growth",
                Type = ProductType.Plan,
                MonthlyPriceCents = 4900,
                IsActive = true
            }
        });

        var service = new AgreementService(agreements, orders, users, products);
        return (service, agreements, orders, users);
    }

    private static Agreement MakeDraftAgreement(string orderId, string userId) => new()
    {
        Id = "agreement-1",
        OrderId = orderId,
        UserId = userId,
        Status = AgreementStatus.Draft,
        ContentSnapshot = "test agreement"
    };

    [Fact]
    public async Task RequestSignatureAsync_MovesDraftToAwaitingSignature()
    {
        var (service, agreements, _, _) = Build();
        await agreements.CreateAsync(MakeDraftAgreement("order-1", "user-1"));

        var result = await service.RequestSignatureAsync("agreement-1");

        Assert.Equal(AgreementStatus.AwaitingSignature, result.Status);
        Assert.Contains(result.AuditTrail, e => e.Action == "SentForSignature");
    }

    [Fact]
    public async Task RequestSignatureAsync_ThrowsWhenAgreementIsNotDraft()
    {
        var (service, agreements, _, _) = Build();
        var agreement = MakeDraftAgreement("order-1", "user-1");
        agreement.Status = AgreementStatus.Signed;
        await agreements.CreateAsync(agreement);

        await Assert.ThrowsAsync<InvalidAgreementStateException>(() =>
            service.RequestSignatureAsync("agreement-1"));
    }

    [Fact]
    public async Task RequestSignatureAsync_ThrowsWhenAgreementDoesNotExist()
    {
        var (service, _, _, _) = Build();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.RequestSignatureAsync("missing"));
    }

    [Fact]
    public async Task SignAsync_MovesAwaitingSignatureStraightToExecuted()
    {
        var (service, agreements, orders, users) = Build();
        await users.CreateAsync(new User { Id = "user-1", Email = "jane@example.com", CurrentSeats = 0 });
        await orders.CreateAsync(new Order
        {
            Id = "order-1",
            UserId = "user-1",
            Lines = new List<OrderLine>
            {
                new() { ProductId = "plan-growth", ProductName = "Growth", UnitPriceCents = 4900, Quantity = 5 }
            },
            Status = OrderStatus.Pending
        });
        var agreement = MakeDraftAgreement("order-1", "user-1");
        agreement.Status = AgreementStatus.AwaitingSignature;
        await agreements.CreateAsync(agreement);

        var result = await service.SignAsync("agreement-1", "jane@example.com", "data:image/png;base64,abc");

        Assert.Equal(AgreementStatus.Executed, result.Status);
        Assert.NotNull(result.SignedAt);
        Assert.NotNull(result.ExecutedAt);
        Assert.Contains(result.AuditTrail, e => e.Action == "Signed");
        Assert.Contains(result.AuditTrail, e => e.Action == "Executed");
    }

    [Fact]
    public async Task SignAsync_AppliesEntitlementsToUserAndOrder()
    {
        var (service, agreements, orders, users) = Build();
        await users.CreateAsync(new User { Id = "user-1", Email = "jane@example.com", CurrentSeats = 0 });
        await orders.CreateAsync(new Order
        {
            Id = "order-1",
            UserId = "user-1",
            Lines = new List<OrderLine>
            {
                new() { ProductId = "plan-growth", ProductName = "Growth", UnitPriceCents = 4900, Quantity = 7 }
            },
            Status = OrderStatus.Pending
        });
        var agreement = MakeDraftAgreement("order-1", "user-1");
        agreement.Status = AgreementStatus.AwaitingSignature;
        await agreements.CreateAsync(agreement);

        await service.SignAsync("agreement-1", "jane@example.com", "data:image/png;base64,abc");

        var updatedUser = await users.GetByIdAsync("user-1");
        var updatedOrder = await orders.GetByIdAsync("order-1");

        Assert.Equal("plan-growth", updatedUser!.CurrentPlanId);
        Assert.Equal(7, updatedUser.CurrentSeats);
        Assert.Equal(OrderStatus.Completed, updatedOrder!.Status);
    }

    [Fact]
    public async Task SignAsync_ThrowsWhenAgreementIsStillDraft()
    {
        var (service, agreements, _, _) = Build();
        await agreements.CreateAsync(MakeDraftAgreement("order-1", "user-1"));

        await Assert.ThrowsAsync<InvalidAgreementStateException>(() =>
            service.SignAsync("agreement-1", "jane@example.com", "data:image/png;base64,abc"));
    }
}
