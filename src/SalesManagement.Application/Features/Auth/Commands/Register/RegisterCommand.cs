using FluentValidation;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.DTOs.Auth;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Enums;
using SalesManagement.Domain.Repositories;
using ValidationException = SalesManagement.Application.Common.Exceptions.ValidationException;

namespace SalesManagement.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Name,
    string Email,
    string Password,
    UserRole Role = UserRole.User) : IRequest<AuthResponse>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(150).WithMessage("Name must not exceed 150 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken);
        if (emailExists)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Email), "Email is already registered.")
            });
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new User(request.Name, request.Email, passwordHash, request.Role);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse(
            Token: token,
            TokenType: "Bearer",
            ExpiresAt: DateTime.UtcNow.AddHours(8),
            User: UserDto.FromEntity(user));
    }
}
