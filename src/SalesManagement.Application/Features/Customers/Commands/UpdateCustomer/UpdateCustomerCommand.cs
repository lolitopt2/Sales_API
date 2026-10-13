using FluentValidation;
using MediatR;
using SalesManagement.Application.DTOs.Customers;
using SalesManagement.Domain.Exceptions;
using SalesManagement.Domain.Repositories;
using ValidationException = SalesManagement.Application.Common.Exceptions.ValidationException;

namespace SalesManagement.Application.Features.Customers.Commands.UpdateCustomer;

public record UpdateCustomerCommand(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    string DocumentNumber,
    string Address,
    bool? IsActive) : IRequest<CustomerDto>;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(150).WithMessage("Name cannot exceed 150 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(200).WithMessage("Email cannot exceed 200 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(30).WithMessage("Phone cannot exceed 30 characters.");

        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("Document/Tax ID number is required.")
            .MaximumLength(50).WithMessage("Document number cannot exceed 50 characters.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(300).WithMessage("Address cannot exceed 300 characters.");
    }
}

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
        {
            throw new EntityNotFoundException(nameof(Domain.Entities.Customer), request.Id);
        }

        // Check unique email if modified
        var existingEmail = await _customerRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingEmail != null && existingEmail.Id != request.Id)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Email), "Another customer with this email already exists.")
            });
        }

        // Check unique doc if modified
        var existingDoc = await _customerRepository.GetByDocumentNumberAsync(request.DocumentNumber, cancellationToken);
        if (existingDoc != null && existingDoc.Id != request.Id)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.DocumentNumber), "Another customer with this document number already exists.")
            });
        }

        customer.Update(
            request.Name,
            request.Email,
            request.Phone,
            request.DocumentNumber,
            request.Address);

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value) customer.Activate();
            else customer.Deactivate();
        }

        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CustomerDto.FromEntity(customer);
    }
}
