using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.DTOs.Auth;
using SalesManagement.Application.Features.Auth.Commands.Login;
using SalesManagement.Application.Features.Auth.Commands.Register;
using SalesManagement.Application.Features.Auth.Queries.GetMe;
using SalesManagement.Domain.Enums;

namespace SalesManagement.Api.Controllers;

public class AuthController : ApiControllerBase
{
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var role = UserRole.User;
        if (!string.IsNullOrWhiteSpace(request.Role) && Enum.TryParse<UserRole>(request.Role, true, out var parsedRole))
        {
            role = parsedRole;
        }

        var command = new RegisterCommand(request.Name, request.Email, request.Password, role);
        var result = await Mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT Bearer token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await Mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves current authenticated user profile and roles.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var query = new GetMeQuery(userId);
        var result = await Mediator.Send(query, cancellationToken);

        return Ok(result);
    }
}
