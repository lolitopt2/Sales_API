using SalesManagement.Domain.Common;
using SalesManagement.Domain.Exceptions;

namespace SalesManagement.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; private set; }
    public Sale Sale { get; private set; } = null!;

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice => Quantity * UnitPrice;

    protected SaleItem() { }

    public SaleItem(Guid saleId, Guid productId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new DomainException("Sale item quantity must be greater than zero.");

        if (unitPrice < 0)
            throw new DomainException("Unit price cannot be negative.");

        SaleId = saleId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
