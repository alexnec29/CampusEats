using CampusEats.Api.Common;
using CampusEats.API.Infrastructure.Repositories;
using CampusEats.Api.Utils.JwtUtil;
using CampusEats.Api.Validators;
using FluentValidation;
using MediatR;

namespace CampusEats.Api.Features.User;

public class LoginUserHandler(IUserRepository userRepository, IJwtService<Models.User> jwtService, LoginUserValidator validator) : IRequestHandler<LoginUserRequest, IResult>
{
    public async Task<IResult> Handle(LoginUserRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(request, cancellationToken);
        Models.User? user = await userRepository.GetByUsernameAsync(request.Username);
        if (user == null)
        {
            return Results.NotFound();
        }

        if (request.Password != request.ConfirmPassword)
        {
            return Results.BadRequest("Passwords do not match");
            
        }
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
        {
            return Results.Unauthorized();
        }
        
        string jwt = jwtService.GenerateToken(user);
        
        return new LoginUserResponse(jwt);
    }
}