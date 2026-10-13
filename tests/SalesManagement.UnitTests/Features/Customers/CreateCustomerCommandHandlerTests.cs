using FluentAssertions;
using Moq;
using SalesManagement.Application.Common.Exceptions;
using SalesManagement.Application.Features.Customers.Commands.CreateCustomer;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Repositories;
using Xunit;

namespace SalesManagement.UnitTests.Features.Customers;

public class CreateCustomerCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateCustomerCommandHandler(_customerRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithUniqueEmailAndDoc_ShouldCreateCustomerSuccessfully()
    {
        // Arrange
        var command = new CreateCustomerCommand(
            "Acme Global",
            "contact@acme.com",
            "+351 912 000 111",
            "PT509999999",
            "Lisbon Tech Hub");

        _customerRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        _customerRepositoryMock
            .Setup(r => r.GetByDocumentNumberAsync(command.DocumentNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Acme Global");
        result.Email.Should().Be("contact@acme.com");
        result.DocumentNumber.Should().Be("PT509999999");

        _customerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateCustomerCommand(
            "Acme Global",
            "contact@acme.com",
            "+351 912 000 111",
            "PT509999999",
            "Lisbon Tech Hub");

        var existingCustomer = new Customer("Existing", "contact@acme.com", "999", "PT111", "Addr");

        _customerRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        _customerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
