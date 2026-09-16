using Pactly.Core.Domain;

namespace Pactly.Core.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetActiveAsync();
    Task<Product?> GetByIdAsync(string id);
    Task<Product?> GetBySlugAsync(string slug);
}
