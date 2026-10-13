using FluentValidation;
using MediatR;
using SalesManagement.Application.DTOs.Customers;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Repositories;
using ValidationException = SalesManagement.Application.Common.Exceptions.ValidationException;

namespace SalesManagement.Application.Features.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string Name,
    string Email,
    string Phone,
    string DocumentNumber,
    string Address) : IRequest<CustomerDto>;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
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

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var existingEmail = await _customerRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingEmail != null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Email), "A customer with this email already exists.")
            });
        }

        var existingDoc = await _customerRepository.GetByDocumentNumberAsync(request.DocumentNumber, cancellationToken);
        if (existingDoc != null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.DocumentNumber), "A customer with this document number already exists.")
            });
        }

        var customer = new Customer(
            request.Name,
            request.Email,
            request.Phone,
            request.DocumentNumber,
            request.Address);

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CustomerDto.FromEntity(customer);
    }
}
