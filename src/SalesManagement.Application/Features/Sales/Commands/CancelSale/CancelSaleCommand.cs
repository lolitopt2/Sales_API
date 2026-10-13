using MediatR;
using SalesManagement.Application.DTOs.Sales;
using SalesManagement.Domain.Enums;
using SalesManagement.Domain.Exceptions;
using SalesManagement.Domain.Repositories;

namespace SalesManagement.Application.Features.Sales.Commands.CancelSale;

public record CancelSaleCommand(Guid Id) : IRequest<SaleDto>;

public class CancelSaleCommandHandler : IRequestHandler<CancelSaleCommand, SaleDto>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelSaleCommandHandler(
        ISaleRepository saleRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SaleDto> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (sale == null)
        {
            throw new EntityNotFoundException(nameof(Domain.Entities.Sale), request.Id);
        }

        if (sale.Status == SaleStatus.Cancelled)
        {
            throw new DomainException("Sale is already cancelled.");
        }

        // Restore stock
        foreach (var item in sale.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product != null)
            {
                product.RestoreStock(item.Quantity);
                _productRepository.Update(product);
            }
        }

        sale.Cancel();
        _saleRepository.Update(sale);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SaleDto.FromEntity(sale);
    }
}
