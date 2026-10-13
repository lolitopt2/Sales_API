using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Enums;

namespace SalesManagement.Application.DTOs.Sales;

public record SaleItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice)
{
    public static SaleItemDto FromEntity(SaleItem item) =>
        new(
            item.Id,
            item.ProductId,
            item.Product?.Name ?? string.Empty,
            item.Product?.Sku ?? string.Empty,
            item.Quantity,
            item.UnitPrice,
            item.TotalPrice);
}

public record SaleDto(
    Guid Id,
    string SaleNumber,
    Guid CustomerId,
    string CustomerName,
    Guid UserId,
    string UserName,
    DateTime SaleDate,
    SaleStatus Status,
    decimal TotalAmount,
    IReadOnlyList<SaleItemDto> Items)
{
    public static SaleDto FromEntity(Sale sale) =>
        new(
            sale.Id,
            sale.SaleNumber,
            sale.CustomerId,
            sale.Customer?.Name ?? string.Empty,
            sale.UserId,
            sale.User?.Name ?? string.Empty,
            sale.SaleDate,
            sale.Status,
            sale.TotalAmount,
            sale.Items.Select(SaleItemDto.FromEntity).ToList());
}

public record CreateSaleItemRequest(
    Guid ProductId,
    int Quantity);

public record CreateSaleRequest(
    Guid CustomerId,
    List<CreateSaleItemRequest> Items);
