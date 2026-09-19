using MongoDB.Bson;
using Pactly.Core.Domain;
using Pactly.Core.Repositories;

namespace Pactly.Core.Services;

public class ProductUnavailableException : Exception
{
    public ProductUnavailableException(string productId)
        : base($"Product '{productId}' does not exist or is no longer available.")
    {
    }
}

public class CartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public CartService(ICartRepository cartRepository, IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<Cart> GetOrCreateCartAsync(string userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is not null)
        {
            return cart;
        }

        return new Cart
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            Lines = new List<CartLine>()
        };
    }

    public async Task<Cart> AddToCartAsync(string userId, string productId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");
        }

        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null || !product.IsActive)
        {
            throw new ProductUnavailableException(productId);
        }

        if (product.SeatLimit is int seatLimit && quantity > seatLimit)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity), $"{product.Name} allows at most {seatLimit} seats.");
        }

        var cart = await GetOrCreateCartAsync(userId);
        var existingLine = cart.Lines.FirstOrDefault(l => l.ProductId == productId);
        if (existingLine is not null)
        {
            existingLine.Quantity = quantity;
        }
        else
        {
            cart.Lines.Add(new CartLine { ProductId = productId, Quantity = quantity });
        }

        await _cartRepository.SaveAsync(cart);
        return cart;
    }

    public async Task<Cart> RemoveFromCartAsync(string userId, string productId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        cart.Lines.RemoveAll(l => l.ProductId == productId);
        await _cartRepository.SaveAsync(cart);
        return cart;
    }
}
