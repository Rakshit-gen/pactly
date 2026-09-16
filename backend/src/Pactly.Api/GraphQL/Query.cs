using System.Security.Claims;
using HotChocolate;
using HotChocolate.Authorization;
using Pactly.Core.Domain;
using Pactly.Core.Repositories;
using Pactly.Core.Services;

namespace Pactly.Api.GraphQL;

public class Query
{
    public Task<List<Product>> GetProducts([Service] CatalogService catalogService) =>
        catalogService.GetProductsAsync();

    public Task<Product?> GetProduct(string slug, [Service] CatalogService catalogService) =>
        catalogService.GetProductBySlugAsync(slug);

    [Authorize]
    public async Task<User?> GetMe(ClaimsPrincipal claimsPrincipal, [Service] IUserRepository userRepository)
    {
        var userId = claimsPrincipal.GetUserId();
        return userId is null ? null : await userRepository.GetByIdAsync(userId);
    }

    [Authorize]
    public Task<Cart> GetCart(ClaimsPrincipal claimsPrincipal, [Service] CartService cartService) =>
        cartService.GetOrCreateCartAsync(claimsPrincipal.RequireUserId());

    [Authorize]
    public Task<List<Order>> GetMyOrders(ClaimsPrincipal claimsPrincipal, [Service] IOrderRepository orderRepository) =>
        orderRepository.GetByUserIdAsync(claimsPrincipal.RequireUserId());

    [Authorize]
    public Task<List<Agreement>> GetMyAgreements(
        ClaimsPrincipal claimsPrincipal,
        [Service] IAgreementRepository agreementRepository) =>
        agreementRepository.GetByUserIdAsync(claimsPrincipal.RequireUserId());

    [Authorize]
    public async Task<Agreement?> GetAgreement(
        string id,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAgreementRepository agreementRepository)
    {
        var userId = claimsPrincipal.RequireUserId();
        var agreement = await agreementRepository.GetByIdAsync(id);
        return agreement is not null && agreement.UserId == userId ? agreement : null;
    }
}
