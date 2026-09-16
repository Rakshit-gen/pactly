using Pactly.Core.Domain;
using Pactly.Core.Services;
using Pactly.Tests.Fakes;
using Xunit;

namespace Pactly.Tests.Services;

public class CatalogServiceTests
{
    private static Product MakePlan(string id, string slug, bool active = true) => new()
    {
        Id = id,
        Slug = slug,
        Name = slug,
        Type = ProductType.Plan,
        MonthlyPriceCents = 1000,
        AnnualPriceCents = 10000,
        IsActive = active
    };

    [Fact]
    public async Task GetProductsAsync_OnlyReturnsActiveProducts()
    {
        var repository = new InMemoryProductRepository(new[]
        {
            MakePlan("1", "starter"),
            MakePlan("2", "retired", active: false)
        });
        var service = new CatalogService(repository);

        var products = await service.GetProductsAsync();

        Assert.Single(products);
        Assert.Equal("starter", products[0].Slug);
    }

    [Fact]
    public async Task GetProductBySlugAsync_ReturnsNull_WhenNotFound()
    {
        var service = new CatalogService(new InMemoryProductRepository());

        var product = await service.GetProductBySlugAsync("missing");

        Assert.Null(product);
    }

    [Fact]
    public async Task GetProductBySlugAsync_ReturnsMatch()
    {
        var repository = new InMemoryProductRepository(new[] { MakePlan("1", "growth") });
        var service = new CatalogService(repository);

        var product = await service.GetProductBySlugAsync("growth");

        Assert.NotNull(product);
        Assert.Equal("1", product!.Id);
    }
}
