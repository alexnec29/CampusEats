using CampusEats.API.Infrastructure.Repositories;
using MediatR;

namespace CampusEats.Api.Features.User;

public class LoginUserHandler(IUserRepository userRepository) : IRequestHandler<LoginUserRequest, IResult>
{
    public async Task<IResult> Handle(LoginUserRequest request, CancellationToken cancellationToken)
    {
        // Validator
        Models.User? user = await userRepository.GetByUsernameAsync(request.Username);
        if (user == null)
        {
            return Results.NotFound();
        }
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
        {
            return Results.Unauthorized();
        }

        // This will return the JWT Token 
        return Results.Ok("JWT Token");
    }
}