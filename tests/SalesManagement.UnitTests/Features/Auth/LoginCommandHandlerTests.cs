using FluentAssertions;
using Moq;
using SalesManagement.Application.Common.Exceptions;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Features.Auth.Commands.Login;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Enums;
using SalesManagement.Domain.Repositories;
using Xunit;

namespace SalesManagement.UnitTests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();

        _handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var command = new LoginCommand("admin@sales.com", "Password@123");
        var user = new User("Admin", "admin@sales.com", "hashed_password", UserRole.Admin);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.Verify(command.Password, "hashed_password"))
            .Returns(true);

        _jwtTokenGeneratorMock
            .Setup(j => j.GenerateToken(user))
            .Returns("fake_jwt_token_xyz");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("fake_jwt_token_xyz");
        result.User.Email.Should().Be("admin@sales.com");
        result.User.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand("admin@sales.com", "WrongPassword");
        var user = new User("Admin", "admin@sales.com", "hashed_password", UserRole.Admin);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.Verify(command.Password, "hashed_password"))
            .Returns(false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");
    }
}
