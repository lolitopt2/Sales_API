using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Domain.Repositories;
using SalesManagement.Infrastructure.Authentication;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories;

namespace SalesManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Configure Database
        var databaseProvider = configuration.GetValue<string>("DatabaseProvider") ?? "Sqlite";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            switch (databaseProvider.ToLowerInvariant())
            {
                case "postgresql":
                case "npgsql":
                    options.UseNpgsql(configuration.GetConnectionString("PostgreSqlConnection"));
                    break;
                case "sqlserver":
                    options.UseSqlServer(configuration.GetConnectionString("SqlServerConnection"));
                    break;
                case "sqlite":
                default:
                    var sqliteConn = configuration.GetConnectionString("DefaultConnection") 
                                     ?? "Data Source=salesmanagement.db";
                    options.UseSqlite(sqliteConn);
                    break;
            }
        });

        // 2. Repositories & Unit of Work
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 3. Authentication & Security Services
        var jwtSettings = new JwtSettings();
        configuration.Bind(JwtSettings.SectionName, jwtSettings);
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // 4. JWT Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        // 5. Database Initializer
        services.AddScoped<DbInitializer>();

        return services;
    }
}
