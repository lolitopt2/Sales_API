using FluentValidation;
using MediatR;
using SalesManagement.Application.Common.Exceptions;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.DTOs.Auth;
using SalesManagement.Domain.Repositories;

namespace SalesManagement.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password) : IRequest<AuthResponse>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse(
            Token: token,
            TokenType: "Bearer",
            ExpiresAt: DateTime.UtcNow.AddHours(8),
            User: UserDto.FromEntity(user));
    }
}
