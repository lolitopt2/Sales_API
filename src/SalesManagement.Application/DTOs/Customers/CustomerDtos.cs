using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.DTOs.Customers;

public record CustomerDto(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    string DocumentNumber,
    string Address,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static CustomerDto FromEntity(Customer customer) =>
        new(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.Phone,
            customer.DocumentNumber,
            customer.Address,
            customer.IsActive,
            customer.CreatedAt,
            customer.UpdatedAt);
}

public record CreateCustomerRequest(
    string Name,
    string Email,
    string Phone,
    string DocumentNumber,
    string Address);

public record UpdateCustomerRequest(
    string Name,
    string Email,
    string Phone,
    string DocumentNumber,
    string Address,
    bool? IsActive);
