using MediatR;
using SalesManagement.Domain.Exceptions;
using SalesManagement.Domain.Repositories;

namespace SalesManagement.Application.Features.Customers.Commands.DeleteCustomer;

public record DeleteCustomerCommand(Guid Id) : IRequest<bool>;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, bool>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCustomerCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
        {
            throw new EntityNotFoundException(nameof(Domain.Entities.Customer), request.Id);
        }

        // Soft delete for enterprise safety
        customer.Deactivate();
        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
