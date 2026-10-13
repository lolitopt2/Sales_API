using SalesManagement.Domain.Entities;

namespace SalesManagement.Domain.Repositories;

public interface ISaleRepository
{
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Sale?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sale>> GetAllAsync(Guid? customerId, Guid? userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid? customerId, Guid? userId, CancellationToken cancellationToken = default);
    Task AddAsync(Sale sale, CancellationToken cancellationToken = default);
    void Update(Sale sale);
}
