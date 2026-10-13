using FluentValidation;
using MediatR;
using SalesManagement.Application.DTOs.Sales;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Exceptions;
using SalesManagement.Domain.Repositories;

namespace SalesManagement.Application.Features.Sales.Commands.CreateSale;

public record CreateSaleCommand(
    Guid CustomerId,
    Guid UserId,
    List<CreateSaleItemRequest> Items) : IRequest<SaleDto>;

public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one sale item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });
    }
}

public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, SaleDto>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSaleCommandHandler(
        ISaleRepository saleRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _saleRepository = saleRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SaleDto> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null || !customer.IsActive)
        {
            throw new EntityNotFoundException(nameof(Domain.Entities.Customer), request.CustomerId);
        }

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null || !user.IsActive)
        {
            throw new EntityNotFoundException(nameof(Domain.Entities.User), request.UserId);
        }

        var saleNumber = $"SL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        var sale = new Sale(saleNumber, customer.Id, user.Id);

        foreach (var itemReq in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemReq.ProductId, cancellationToken);
            if (product == null || !product.IsActive)
            {
                throw new EntityNotFoundException(nameof(Domain.Entities.Product), itemReq.ProductId);
            }

            sale.AddItem(product, itemReq.Quantity);
            _productRepository.Update(product);
        }

        await _saleRepository.AddAsync(sale, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch complete entity with navigation properties for DTO mapping
        var createdSale = await _saleRepository.GetByIdWithDetailsAsync(sale.Id, cancellationToken);
        return SaleDto.FromEntity(createdSale ?? sale);
    }
}
