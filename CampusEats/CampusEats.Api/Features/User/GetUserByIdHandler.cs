using CampusEats.Api.Infrastructure.Repositories;
using MediatR;

namespace CampusEats.Api.Features.User;

public class GetUserByIdHandler(IUserRepository userRepository) : IRequestHandler<GetUserByIdRequest, IResult>
{
    public async Task<IResult> Handle(GetUserByIdRequest request, CancellationToken cancellationToken)
    {
        Models.User? user = await userRepository.GetByIdAsync(request.Id);
        if (user == null)
        {
            return Results.NotFound($"User with ID: {request.Id}, not found");
        }

        GetUserByIdResponse response = new GetUserByIdResponse
        {
            Username = user.Username,
            Email = user.Email
        };
        
        return Results.Ok(response);
    }
}