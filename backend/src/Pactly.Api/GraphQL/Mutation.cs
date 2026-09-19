using System.Security.Claims;
using HotChocolate;
using HotChocolate.Authorization;
using Pactly.Api.DTOs;
using Pactly.Api.Security;
using Pactly.Core.Domain;
using Pactly.Core.Repositories;
using Pactly.Core.Services;

namespace Pactly.Api.GraphQL;

public class Mutation
{
    public async Task<AuthPayload> Register(
        string email,
        string password,
        string displayName,
        [Service] AuthService authService,
        [Service] TokenService tokenService)
    {
        var user = await authService.RegisterAsync(email, password, displayName);
        return new AuthPayload(tokenService.CreateToken(user), user);
    }

    public async Task<AuthPayload> Login(
        string email,
        string password,
        [Service] AuthService authService,
        [Service] TokenService tokenService)
    {
        var user = await authService.ValidateCredentialsAsync(email, password);
        if (user is null)
        {
            throw new GraphQLException("Email or password is incorrect.");
        }

        return new AuthPayload(tokenService.CreateToken(user), user);
    }

    [Authorize]
    public Task<Cart> AddToCart(
        string productId,
        int quantity,
        ClaimsPrincipal claimsPrincipal,
        [Service] CartService cartService) =>
        cartService.AddToCartAsync(claimsPrincipal.RequireUserId(), productId, quantity);

    [Authorize]
    public Task<Cart> RemoveFromCart(
        string productId,
        ClaimsPrincipal claimsPrincipal,
        [Service] CartService cartService) =>
        cartService.RemoveFromCartAsync(claimsPrincipal.RequireUserId(), productId);

    [Authorize]
    public async Task<CheckoutPayload> Checkout(
        ClaimsPrincipal claimsPrincipal,
        [Service] CheckoutService checkoutService)
    {
        var (order, agreement) = await checkoutService.CheckoutAsync(claimsPrincipal.RequireUserId());
        return new CheckoutPayload(order, agreement);
    }

    [Authorize]
    public async Task<Agreement> SignAgreement(
        string agreementId,
        string signatureDataUrl,
        ClaimsPrincipal claimsPrincipal,
        [Service] AgreementService agreementService,
        [Service] IUserRepository userRepository,
        [Service] IAgreementRepository agreementRepository)
    {
        var userId = claimsPrincipal.RequireUserId();
        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new GraphQLException("Signed in user no longer exists.");

        // Same not-found answer for "missing" and "someone else's" so ids can't be probed.
        var agreement = await agreementRepository.GetByIdAsync(agreementId);
        if (agreement is null || agreement.UserId != userId)
        {
            throw new GraphQLException("Agreement not found.");
        }

        return await agreementService.SignAsync(agreementId, user.Email, signatureDataUrl);
    }
}
