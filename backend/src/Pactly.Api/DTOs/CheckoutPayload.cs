using Pactly.Core.Domain;

namespace Pactly.Api.DTOs;

public record CheckoutPayload(Order Order, Agreement Agreement);
