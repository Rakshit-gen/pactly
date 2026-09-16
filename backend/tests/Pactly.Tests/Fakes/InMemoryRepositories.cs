using Pactly.Core.Domain;
using Pactly.Core.Repositories;

namespace Pactly.Tests.Fakes;

public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products;

    public InMemoryProductRepository(IEnumerable<Product>? seed = null)
    {
        _products = seed?.ToList() ?? new List<Product>();
    }

    public Task<List<Product>> GetActiveAsync() =>
        Task.FromResult(_products.Where(p => p.IsActive).ToList());

    public Task<Product?> GetByIdAsync(string id) =>
        Task.FromResult(_products.FirstOrDefault(p => p.Id == id));

    public Task<Product?> GetBySlugAsync(string slug) =>
        Task.FromResult(_products.FirstOrDefault(p => p.Slug == slug));

    public void Add(Product product) => _products.Add(product);
}

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public Task<User?> GetByIdAsync(string id) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByEmailAsync(string email) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Email == email));

    public Task CreateAsync(User user)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user)
    {
        var index = _users.FindIndex(u => u.Id == user.Id);
        if (index >= 0)
        {
            _users[index] = user;
        }

        return Task.CompletedTask;
    }
}

public class InMemoryCartRepository : ICartRepository
{
    private readonly Dictionary<string, Cart> _cartsByUserId = new();

    public Task<Cart?> GetByUserIdAsync(string userId) =>
        Task.FromResult(_cartsByUserId.TryGetValue(userId, out var cart) ? cart : null);

    public Task SaveAsync(Cart cart)
    {
        _cartsByUserId[cart.UserId] = cart;
        return Task.CompletedTask;
    }

    public Task ClearAsync(string userId)
    {
        _cartsByUserId.Remove(userId);
        return Task.CompletedTask;
    }
}

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = new();

    public Task<Order?> GetByIdAsync(string id) =>
        Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));

    public Task<List<Order>> GetByUserIdAsync(string userId) =>
        Task.FromResult(_orders.Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt).ToList());

    public Task CreateAsync(Order order)
    {
        _orders.Add(order);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Order order)
    {
        var index = _orders.FindIndex(o => o.Id == order.Id);
        if (index >= 0)
        {
            _orders[index] = order;
        }

        return Task.CompletedTask;
    }
}

public class InMemoryAgreementRepository : IAgreementRepository
{
    private readonly List<Agreement> _agreements = new();

    public Task<Agreement?> GetByIdAsync(string id) =>
        Task.FromResult(_agreements.FirstOrDefault(a => a.Id == id));

    public Task<List<Agreement>> GetByUserIdAsync(string userId) =>
        Task.FromResult(_agreements.Where(a => a.UserId == userId).ToList());

    public Task CreateAsync(Agreement agreement)
    {
        _agreements.Add(agreement);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Agreement agreement)
    {
        var index = _agreements.FindIndex(a => a.Id == agreement.Id);
        if (index >= 0)
        {
            _agreements[index] = agreement;
        }

        return Task.CompletedTask;
    }
}
