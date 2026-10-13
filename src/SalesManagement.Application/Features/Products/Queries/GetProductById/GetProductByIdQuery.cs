using MediatR;
using SalesManagement.Application.DTOs.Products;
using SalesManagement.Domain.Exceptions;
using SalesManagement.Domain.Repositories;

namespace SalesManagement.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            throw new EntityNotFoundException(nameof(Domain.Entities.Product), request.Id);
        }

        return ProductDto.FromEntity(product);
    }
}
