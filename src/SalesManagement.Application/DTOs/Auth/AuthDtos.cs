namespace SalesManagement.Application.DTOs.Auth;

public record AuthResponse(
    string Token,
    string TokenType,
    DateTime ExpiresAt,
    UserDto User);

public record RegisterRequest(
    string Name,
    string Email,
    string Password,
    string? Role);

public record LoginRequest(
    string Email,
    string Password);
