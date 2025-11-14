using CampusEats.API.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Validators;
using MediatR;

namespace CampusEats.Api.Features.User;

public class CreateUserHandler(IUserRepository userRepository, CreateUserValidator validator) : IRequestHandler<CreateUserRequest, IResult>
{
    public async Task<IResult> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(request, cancellationToken);
        Models.User? user = await userRepository.GetByUsernameAsync(request.Username);
        if (user != null)
        {
            return Results.Conflict();
        }
        if (request.Password != request.ConfirmPassword)
        {
            return Results.BadRequest("Passwords do not match");
        }
        Models.User newUser = new Models.User
        {
            Id = new Guid(),
            Username = request.Username,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Email = request.Email,
            Role = Role.Buyer
        };

        await userRepository.AddAsync(newUser);
        
        return Results.Created();
    }
}