using SalesManagement.Domain.Entities;

namespace SalesManagement.Domain.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Customer?> GetByDocumentNumberAsync(string documentNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> GetAllAsync(string? searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? searchTerm, CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    void Update(Customer customer);
    void Delete(Customer customer);
}
