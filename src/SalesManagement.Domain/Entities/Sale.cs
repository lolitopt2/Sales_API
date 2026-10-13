using SalesManagement.Domain.Common;
using SalesManagement.Domain.Enums;
using SalesManagement.Domain.Exceptions;

namespace SalesManagement.Domain.Entities;

public class Sale : BaseEntity
{
    public string SaleNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public DateTime SaleDate { get; private set; } = DateTime.UtcNow;
    public SaleStatus Status { get; private set; } = SaleStatus.Pending;
    public decimal TotalAmount { get; private set; }

    private readonly List<SaleItem> _items = new();
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    protected Sale() { }

    public Sale(string saleNumber, Guid customerId, Guid userId)
    {
        SaleNumber = saleNumber.Trim();
        CustomerId = customerId;
        UserId = userId;
        SaleDate = DateTime.UtcNow;
        Status = SaleStatus.Completed;
        TotalAmount = 0;
    }

    public void AddItem(Product product, int quantity)
    {
        if (Status == SaleStatus.Cancelled)
            throw new DomainException("Cannot add items to a cancelled sale.");

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (!product.IsActive)
            throw new DomainException($"Cannot add inactive product '{product.Name}' to sale.");

        // Deduct inventory
        product.ReduceStock(quantity);

        var item = new SaleItem(Id, product.Id, quantity, product.Price);
        _items.Add(item);

        TotalAmount += item.TotalPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == SaleStatus.Cancelled)
            throw new DomainException("Sale is already cancelled.");

        Status = SaleStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        // Note: Inventory restoration should be performed by the domain/application service using item references.
    }
}
