using HotChocolate.Types;
using Pactly.Core.Domain;
using Pactly.Core.Repositories;

namespace Pactly.Api.GraphQL;

public class CartLineType : ObjectType<CartLine>
{
    protected override void Configure(IObjectTypeDescriptor<CartLine> descriptor)
    {
        descriptor
            .Field("product")
            .Type<ObjectType<Product>>()
            .Resolve(async context =>
            {
                var cartLine = context.Parent<CartLine>();
                var productRepository = context.Service<IProductRepository>();
                return await productRepository.GetByIdAsync(cartLine.ProductId);
            });
    }
}
