using FluentValidation;
using MediatR;
using SalesManagement.Application.DTOs.Products;
using SalesManagement.Domain.Exceptions;
using SalesManagement.Domain.Repositories;

namespace SalesManagement.Application.Features.Products.Commands.UpdateStock;

public record UpdateStockCommand(
    Guid Id,
    int StockQuantity) : IRequest<ProductDto>;

public class UpdateStockCommandValidator : AbstractValidator<UpdateStockCommand>
{
    public UpdateStockCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");
    }
}

public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStockCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            throw new EntityNotFoundException(nameof(Domain.Entities.Product), request.Id);
        }

        product.SetStock(request.StockQuantity);

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductDto.FromEntity(product);
    }
}
