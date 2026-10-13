using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Enums;

namespace SalesManagement.Application.DTOs.Auth;

public record UserDto(
    Guid Id,
    string Name,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt)
{
    public static UserDto FromEntity(User user) =>
        new(user.Id, user.Name, user.Email, user.Role, user.IsActive, user.CreatedAt);
}
