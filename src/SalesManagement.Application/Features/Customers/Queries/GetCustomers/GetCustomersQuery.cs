using MediatR;
using SalesManagement.Application.Common.Models;
using SalesManagement.Application.DTOs.Customers;
using SalesManagement.Domain.Repositories;

namespace SalesManagement.Application.Features.Customers.Queries.GetCustomers;

public record GetCustomersQuery(
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<CustomerDto>>;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PagedResult<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomersQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<PagedResult<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 100);

        var items = await _customerRepository.GetAllAsync(request.SearchTerm, pageNumber, pageSize, cancellationToken);
        var totalCount = await _customerRepository.CountAsync(request.SearchTerm, cancellationToken);

        var dtos = items.Select(CustomerDto.FromEntity).ToList();

        return new PagedResult<CustomerDto>(dtos, totalCount, pageNumber, pageSize);
    }
}
