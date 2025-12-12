using CampusEats.Api.Infrastructure.Repositories;
using MediatR;

namespace CampusEats.Api.Features.User;

public class GetAllUsersHandler(IUserRepository userRepository)
    : IRequestHandler<GetAllUsersRequest, IResult>
{
    public async Task<IResult> Handle(GetAllUsersRequest request, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync();

        var response = users.Select(u => new GetAllUsersResponse
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Role = u.Role.ToString()
        }).ToList();

        return Results.Ok(response);
    }
}