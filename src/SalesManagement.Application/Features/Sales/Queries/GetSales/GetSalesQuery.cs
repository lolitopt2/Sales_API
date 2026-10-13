using MediatR;
using SalesManagement.Application.Common.Models;
using SalesManagement.Application.DTOs.Sales;
using SalesManagement.Domain.Repositories;

namespace SalesManagement.Application.Features.Sales.Queries.GetSales;

public record GetSalesQuery(
    Guid? CustomerId = null,
    Guid? UserId = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<SaleDto>>;

public class GetSalesQueryHandler : IRequestHandler<GetSalesQuery, PagedResult<SaleDto>>
{
    private readonly ISaleRepository _saleRepository;

    public GetSalesQueryHandler(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<PagedResult<SaleDto>> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 100);

        var items = await _saleRepository.GetAllAsync(request.CustomerId, request.UserId, pageNumber, pageSize, cancellationToken);
        var totalCount = await _saleRepository.CountAsync(request.CustomerId, request.UserId, cancellationToken);

        var dtos = items.Select(SaleDto.FromEntity).ToList();

        return new PagedResult<SaleDto>(dtos, totalCount, pageNumber, pageSize);
    }
}
