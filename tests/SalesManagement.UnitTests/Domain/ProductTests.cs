using FluentAssertions;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Exceptions;
using Xunit;

namespace SalesManagement.UnitTests.Domain;

public class ProductTests
{
    [Fact]
    public void ReduceStock_WithValidQuantity_ShouldDeductStock()
    {
        // Arrange
        var product = new Product("Mechanical Keyboard", "RGB Gaming", "KB-01", 89.99m, 20);

        // Act
        product.ReduceStock(5);

        // Assert
        product.StockQuantity.Should().Be(15);
    }

    [Fact]
    public void ReduceStock_WithQuantityGreaterThanStock_ShouldThrowInsufficientStockException()
    {
        // Arrange
        var product = new Product("Mechanical Keyboard", "RGB Gaming", "KB-01", 89.99m, 5);

        // Act
        var act = () => product.ReduceStock(10);

        // Assert
        act.Should().Throw<InsufficientStockException>();
    }

    [Fact]
    public void RestoreStock_WithValidQuantity_ShouldIncreaseStock()
    {
        // Arrange
        var product = new Product("Mechanical Keyboard", "RGB Gaming", "KB-01", 89.99m, 10);

        // Act
        product.RestoreStock(7);

        // Assert
        product.StockQuantity.Should().Be(17);
    }

    [Fact]
    public void Constructor_WithNegativePrice_ShouldThrowDomainException()
    {
        // Act
        var act = () => new Product("Mechanical Keyboard", "RGB Gaming", "KB-01", -10m, 5);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Price cannot be negative.");
    }
}
