namespace SalesManagement.Domain.Exceptions;

public class InsufficientStockException : DomainException
{
    public InsufficientStockException(string productName, int requestedQuantity, int availableStock)
        : base($"Insufficient stock for product '{productName}'. Requested: {requestedQuantity}, Available: {availableStock}.")
    {
    }
}
