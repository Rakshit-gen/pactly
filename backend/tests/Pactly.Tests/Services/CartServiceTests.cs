using Pactly.Core.Domain;
using Pactly.Core.Services;
using Pactly.Tests.Fakes;
using Xunit;

namespace Pactly.Tests.Services;

public class CartServiceTests
{
    private static InMemoryProductRepository ProductRepoWith(params Product[] products) =>
        new(products);

    private static Product MakeProduct(string id = "p1", bool active = true) => new()
    {
        Id = id,
        Slug = id,
        Name = id,
        Type = ProductType.Plan,
        MonthlyPriceCents = 2000,
        IsActive = active
    };

    [Fact]
    public async Task AddToCartAsync_CreatesCart_WhenNoneExists()
    {
        var service = new CartService(new InMemoryCartRepository(), ProductRepoWith(MakeProduct()));

        var cart = await service.AddToCartAsync("user-1", "p1", 3);

        Assert.Single(cart.Lines);
        Assert.Equal(3, cart.Lines[0].Quantity);
    }

    [Fact]
    public async Task AddToCartAsync_UpdatesQuantity_WhenProductAlreadyInCart()
    {
        var service = new CartService(new InMemoryCartRepository(), ProductRepoWith(MakeProduct()));
        await service.AddToCartAsync("user-1", "p1", 3);

        var cart = await service.AddToCartAsync("user-1", "p1", 5);

        Assert.Single(cart.Lines);
        Assert.Equal(5, cart.Lines[0].Quantity);
    }

    [Fact]
    public async Task AddToCartAsync_ThrowsWhenProductDoesNotExist()
    {
        var service = new CartService(new InMemoryCartRepository(), ProductRepoWith());

        await Assert.ThrowsAsync<ProductUnavailableException>(() =>
            service.AddToCartAsync("user-1", "missing", 1));
    }

    [Fact]
    public async Task AddToCartAsync_ThrowsWhenProductIsInactive()
    {
        var service = new CartService(new InMemoryCartRepository(), ProductRepoWith(MakeProduct(active: false)));

        await Assert.ThrowsAsync<ProductUnavailableException>(() =>
            service.AddToCartAsync("user-1", "p1", 1));
    }

    [Fact]
    public async Task AddToCartAsync_ThrowsWhenQuantityIsZeroOrLess()
    {
        var service = new CartService(new InMemoryCartRepository(), ProductRepoWith(MakeProduct()));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.AddToCartAsync("user-1", "p1", 0));
    }

    [Fact]
    public async Task RemoveFromCartAsync_RemovesOnlyMatchingLine()
    {
        var cartRepository = new InMemoryCartRepository();
        var service = new CartService(cartRepository, ProductRepoWith(MakeProduct("p1"), MakeProduct("p2")));
        await service.AddToCartAsync("user-1", "p1", 1);
        await service.AddToCartAsync("user-1", "p2", 1);

        var cart = await service.RemoveFromCartAsync("user-1", "p1");

        Assert.Single(cart.Lines);
        Assert.Equal("p2", cart.Lines[0].ProductId);
    }
}
