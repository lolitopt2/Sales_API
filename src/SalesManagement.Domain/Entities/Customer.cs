using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities;

public class Customer : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string DocumentNumber { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    // Navigation
    private readonly List<Sale> _sales = new();
    public IReadOnlyCollection<Sale> Sales => _sales.AsReadOnly();

    protected Customer() { }

    public Customer(string name, string email, string phone, string documentNumber, string address)
    {
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        Phone = phone.Trim();
        DocumentNumber = documentNumber.Trim();
        Address = address.Trim();
        IsActive = true;
    }

    public void Update(string name, string email, string phone, string documentNumber, string address)
    {
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        Phone = phone.Trim();
        DocumentNumber = documentNumber.Trim();
        Address = address.Trim();
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
