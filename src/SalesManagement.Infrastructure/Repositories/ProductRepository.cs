using Microsoft.EntityFrameworkCore;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Repositories;
using SalesManagement.Infrastructure.Data;

namespace SalesManagement.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Sku == sku.Trim().ToUpperInvariant(), cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(string? searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) ||
                                     p.Sku.ToLower().Contains(term) ||
                                     p.Description.ToLower().Contains(term));
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) ||
                                     p.Sku.ToLower().Contains(term) ||
                                     p.Description.ToLower().Contains(term));
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
    }

    public void Delete(Product product)
    {
        _context.Products.Remove(product);
    }
}
