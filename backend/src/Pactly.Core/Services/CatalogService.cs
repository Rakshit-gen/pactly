using Pactly.Core.Domain;
using Pactly.Core.Repositories;

namespace Pactly.Core.Services;

public class CatalogService
{
    private readonly IProductRepository _productRepository;

    public CatalogService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Task<List<Product>> GetProductsAsync() => _productRepository.GetActiveAsync();

    public Task<Product?> GetProductBySlugAsync(string slug) => _productRepository.GetBySlugAsync(slug);

    public Task<Product?> GetProductByIdAsync(string id) => _productRepository.GetByIdAsync(id);
}
