using Microsoft.EntityFrameworkCore;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Repositories;
using SalesManagement.Infrastructure.Data;

namespace SalesManagement.Infrastructure.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly ApplicationDbContext _context;

    public SaleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Sale?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.User)
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Sale>> GetAllAsync(
        Guid? customerId,
        Guid? userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.User)
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .AsNoTracking()
            .AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(s => s.CustomerId == customerId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(s => s.UserId == userId.Value);
        }

        return await query
            .OrderByDescending(s => s.SaleDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(Guid? customerId, Guid? userId, CancellationToken cancellationToken = default)
    {
        var query = _context.Sales.AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(s => s.CustomerId == customerId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(s => s.UserId == userId.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
    }

    public void Update(Sale sale)
    {
        _context.Sales.Update(sale);
    }
}
