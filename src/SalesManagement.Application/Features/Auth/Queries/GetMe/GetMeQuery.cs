using MediatR;
using SalesManagement.Application.Common.Exceptions;
using SalesManagement.Application.DTOs.Auth;
using SalesManagement.Domain.Repositories;

namespace SalesManagement.Application.Features.Auth.Queries.GetMe;

public record GetMeQuery(Guid UserId) : IRequest<UserDto>;

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    public GetMeQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedException("User not found or account is deactivated.");
        }

        return UserDto.FromEntity(user);
    }
}
