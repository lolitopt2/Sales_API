using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Enums;

namespace SalesManagement.Infrastructure.Data;

public class DbInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ILogger<DbInitializer> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _context.Database.EnsureCreatedAsync();

            if (await _context.Users.AnyAsync())
            {
                return; // Database has already been seeded
            }

            _logger.LogInformation("Database is empty. Seeding initial test data...");

            // 1. Seed Users
            var adminPasswordHash = _passwordHasher.Hash("Admin@123");
            var adminUser = new User("System Administrator", "admin@salesmanagement.com", adminPasswordHash, UserRole.Admin);

            var userPasswordHash = _passwordHasher.Hash("User@123");
            var regularUser = new User("Sales Representative", "user@salesmanagement.com", userPasswordHash, UserRole.User);

            await _context.Users.AddRangeAsync(adminUser, regularUser);
            await _context.SaveChangesAsync();

            // 2. Seed Customers
            var customer1 = new Customer(
                "TechNova Solutions Ltd",
                "contact@technova.com",
                "+351 912 345 678",
                "PT501234567",
                "Avenida da Liberdade 120, Lisboa, Portugal");

            var customer2 = new Customer(
                "Apex Global Logistics",
                "procurement@apexglobal.com",
                "+351 923 456 789",
                "PT509876543",
                "Rua de Santa Catarina 450, Porto, Portugal");

            var customer3 = new Customer(
                "ByteForge Cloud Systems",
                "finance@byteforge.io",
                "+351 934 567 890",
                "PT508765432",
                "Parque Tecnológico de Braga, Lote 4, Braga, Portugal");

            await _context.Customers.AddRangeAsync(customer1, customer2, customer3);
            await _context.SaveChangesAsync();

            // 3. Seed Products
            var prod1 = new Product(
                "Enterprise ERP License",
                "Comprehensive ERP license including modules for CRM, Invoicing and Inventory.",
                "SOFT-ERP-01",
                1250.00m,
                50);

            var prod2 = new Product(
                "Cloud Hosting & DevOps Support",
                "Managed Kubernetes cluster hosting with 24/7 incident response SLA.",
                "SRV-CLOUD-02",
                450.00m,
                100);

            var prod3 = new Product(
                "Cybersecurity Penetration Testing",
                "Full security audit and penetration test report for corporate networks.",
                "SEC-AUDIT-03",
                890.00m,
                25);

            var prod4 = new Product(
                "High-Performance Developer Laptop",
                "64GB RAM, 2TB SSD, Intel Core i9 workstation for software engineering.",
                "HW-DEV-04",
                2199.99m,
                15);

            await _context.Products.AddRangeAsync(prod1, prod2, prod3, prod4);
            await _context.SaveChangesAsync();

            // 4. Seed Initial Sale
            var initialSale = new Sale("SL-20261013-0001", customer1.Id, regularUser.Id);
            initialSale.AddItem(prod1, 1);
            initialSale.AddItem(prod2, 2);

            await _context.Sales.AddAsync(initialSale);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Database seeded successfully with users, customers, products and sales!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
