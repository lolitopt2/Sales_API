using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.Common.Exceptions;
using SalesManagement.Domain.Exceptions;

namespace SalesManagement.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        var response = context.Response;
        response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Instance = context.Request.Path
        };

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Validation Failed";
                problemDetails.Detail = "One or more validation errors occurred.";
                problemDetails.Extensions["errors"] = validationEx.Errors;
                break;

            case EntityNotFoundException notFoundEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                problemDetails.Status = (int)HttpStatusCode.NotFound;
                problemDetails.Title = "Resource Not Found";
                problemDetails.Detail = notFoundEx.Message;
                break;

            case InsufficientStockException stockEx:
                response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                problemDetails.Status = (int)HttpStatusCode.UnprocessableEntity;
                problemDetails.Title = "Insufficient Stock";
                problemDetails.Detail = stockEx.Message;
                break;

            case UnauthorizedException unauthEx:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                problemDetails.Status = (int)HttpStatusCode.Unauthorized;
                problemDetails.Title = "Unauthorized";
                problemDetails.Detail = unauthEx.Message;
                break;

            case DomainException domainEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Domain Rule Violation";
                problemDetails.Detail = domainEx.Message;
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                problemDetails.Title = "Internal Server Error";
                problemDetails.Detail = "An unexpected error occurred. Please contact the administrator.";
                break;
        }

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await response.WriteAsync(json);
    }
}
