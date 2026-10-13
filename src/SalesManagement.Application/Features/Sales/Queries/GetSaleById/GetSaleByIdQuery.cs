using MediatR;
using SalesManagement.Application.DTOs.Sales;
using SalesManagement.Domain.Exceptions;
using SalesManagement.Domain.Repositories;

namespace SalesManagement.Application.Features.Sales.Queries.GetSaleById;

public record GetSaleByIdQuery(Guid Id) : IRequest<SaleDto>;

public class GetSaleByIdQueryHandler : IRequestHandler<GetSaleByIdQuery, SaleDto>
{
    private readonly ISaleRepository _saleRepository;

    public GetSaleByIdQueryHandler(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<SaleDto> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (sale == null)
        {
            throw new EntityNotFoundException(nameof(Domain.Entities.Sale), request.Id);
        }

        return SaleDto.FromEntity(sale);
    }
}
