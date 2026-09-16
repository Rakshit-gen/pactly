using System.Security.Claims;
using HotChocolate;

namespace Pactly.Api.GraphQL;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal principal) =>
        principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public static string RequireUserId(this ClaimsPrincipal principal) =>
        principal.GetUserId() ?? throw new GraphQLException("You must be signed in to do that.");
}
