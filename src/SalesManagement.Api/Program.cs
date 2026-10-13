using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using SalesManagement.Api.Middlewares;
using SalesManagement.Application;
using SalesManagement.Infrastructure;
using SalesManagement.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Application and Infrastructure layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. Configure Controllers and JSON Serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// 3. Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 4. Configure Swagger / OpenAPI with JWT Bearer Authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sales Management API",
        Version = "v1",
        Description = "Enterprise REST API for Customers and Sales Management, built with Clean Architecture, CQRS (MediatR), JWT Bearer Authentication with Roles, FluentValidation, and EF Core.",
        Contact = new OpenApiContact
        {
            Name = "Enterprise Software Portfolio",
            Email = "support@salesmanagement.com"
        }
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token format: Bearer {your token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

// 5. Global Error Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 6. Swagger UI (Always enabled for interactive portfolio exploration)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sales Management API v1");
    c.RoutePrefix = string.Empty; // Serves Swagger UI at application root URL (http://localhost:5000/)
});

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 7. Automatic Database Migration / Seed Data Initialization
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
        await dbInitializer.InitializeAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to initialize and seed database on startup.");
    }
}

app.Run();
