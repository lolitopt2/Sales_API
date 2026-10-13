using MediatR;
using SalesManagement.Application.Common.Models;
using SalesManagement.Application.DTOs.Products;
using SalesManagement.Domain.Repositories;

namespace SalesManagement.Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery(
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<ProductDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 100);

        var items = await _productRepository.GetAllAsync(request.SearchTerm, pageNumber, pageSize, cancellationToken);
        var totalCount = await _productRepository.CountAsync(request.SearchTerm, cancellationToken);

        var dtos = items.Select(ProductDto.FromEntity).ToList();

        return new PagedResult<ProductDto>(dtos, totalCount, pageNumber, pageSize);
    }
}
