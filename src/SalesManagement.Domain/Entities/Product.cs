using SalesManagement.Domain.Common;
using SalesManagement.Domain.Exceptions;

namespace SalesManagement.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; } = true;

    protected Product() { }

    public Product(string name, string description, string sku, decimal price, int stockQuantity)
    {
        if (price < 0)
            throw new DomainException("Price cannot be negative.");

        if (stockQuantity < 0)
            throw new DomainException("Initial stock quantity cannot be negative.");

        Name = name.Trim();
        Description = description.Trim();
        Sku = sku.Trim().ToUpperInvariant();
        Price = price;
        StockQuantity = stockQuantity;
        IsActive = true;
    }

    public void Update(string name, string description, string sku, decimal price)
    {
        if (price < 0)
            throw new DomainException("Price cannot be negative.");

        Name = name.Trim();
        Description = description.Trim();
        Sku = sku.Trim().ToUpperInvariant();
        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReduceStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity to reduce must be greater than zero.");

        if (StockQuantity < quantity)
            throw new InsufficientStockException(Name, quantity, StockQuantity);

        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RestoreStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity to restore must be greater than zero.");

        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStock(int newStock)
    {
        if (newStock < 0)
            throw new DomainException("Stock cannot be negative.");

        StockQuantity = newStock;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
