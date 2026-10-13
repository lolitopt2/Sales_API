using SalesManagement.Domain.Common;
using SalesManagement.Domain.Enums;

namespace SalesManagement.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; } = UserRole.User;
    public bool IsActive { get; private set; } = true;

    // Navigation
    private readonly List<Sale> _sales = new();
    public IReadOnlyCollection<Sale> Sales => _sales.AsReadOnly();

    protected User() { }

    public User(string name, string email, string passwordHash, UserRole role = UserRole.User)
    {
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
    }

    public void UpdateProfile(string name, string email)
    {
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRole(UserRole role)
    {
        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
