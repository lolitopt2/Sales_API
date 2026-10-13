using FluentValidation;
using MediatR;
using SalesManagement.Application.DTOs.Products;
using SalesManagement.Domain.Exceptions;
using SalesManagement.Domain.Repositories;
using ValidationException = SalesManagement.Application.Common.Exceptions.ValidationException;

namespace SalesManagement.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    string Sku,
    decimal Price,
    bool? IsActive) : IRequest<ProductDto>;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(150).WithMessage("Product name cannot exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            throw new EntityNotFoundException(nameof(Domain.Entities.Product), request.Id);
        }

        var existingSku = await _productRepository.GetBySkuAsync(request.Sku, cancellationToken);
        if (existingSku != null && existingSku.Id != request.Id)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Sku), "Another product with this SKU already exists.")
            });
        }

        product.Update(request.Name, request.Description, request.Sku, request.Price);

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value) product.Activate();
            else product.Deactivate();
        }

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductDto.FromEntity(product);
    }
}
