using MediatR;
using SalesManagement.Application.DTOs.Customers;
using SalesManagement.Domain.Exceptions;
using SalesManagement.Domain.Repositories;

namespace SalesManagement.Application.Features.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto>;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByIdQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
        {
            throw new EntityNotFoundException(nameof(Domain.Entities.Customer), request.Id);
        }

        return CustomerDto.FromEntity(customer);
    }
}
