using Microsoft.EntityFrameworkCore;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Repositories;
using SalesManagement.Infrastructure.Data;

namespace SalesManagement.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<Customer?> GetByDocumentNumberAsync(string documentNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.DocumentNumber == documentNumber.Trim(), cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(string? searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(term) ||
                                     c.Email.ToLower().Contains(term) ||
                                     c.DocumentNumber.Contains(term));
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(term) ||
                                     c.Email.ToLower().Contains(term) ||
                                     c.DocumentNumber.Contains(term));
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
    }

    public void Update(Customer customer)
    {
        _context.Customers.Update(customer);
    }

    public void Delete(Customer customer)
    {
        _context.Customers.Remove(customer);
    }
}
