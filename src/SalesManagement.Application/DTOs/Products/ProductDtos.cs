using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.DTOs.Products;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    string Sku,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static ProductDto FromEntity(Product product) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            product.Sku,
            product.Price,
            product.StockQuantity,
            product.IsActive,
            product.CreatedAt,
            product.UpdatedAt);
}

public record CreateProductRequest(
    string Name,
    string Description,
    string Sku,
    decimal Price,
    int StockQuantity);

public record UpdateProductRequest(
    string Name,
    string Description,
    string Sku,
    decimal Price,
    bool? IsActive);

public record UpdateStockRequest(
    int StockQuantity);
