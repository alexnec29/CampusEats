using CampusEats.API.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using MediatR;

namespace CampusEats.Api.Features.User;

public class CreateUserHandler(IUserRepository userRepository) : IRequestHandler<CreateUserRequest, IResult>
{
    public async Task<IResult> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        // Validator
        // Check if username already used
        Models.User newUser = new Models.User
        {
            Id = new Guid(),
            Username = request.Username,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Email = request.Username,
            Role = Role.Buyer
        };

        await userRepository.AddAsync(newUser);
        
        return Results.Created();
    }
}