using CampusEats.Api.Infrastructure.Repositories;
using MediatR;

namespace CampusEats.Api.Features.User
{
    public class UpdateUserRoleHandler : IRequestHandler<UpdateUserRoleRequest, IResult>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserRoleHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IResult> Handle(UpdateUserRoleRequest request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                return Results.NotFound($"User with ID {request.UserId} not found");

            // Parse role from string to enum
            if (!Enum.TryParse(request.Role, true, out Models.Enums.Role newRole))
                return Results.BadRequest($"Invalid role: {request.Role}");

            user.Role = newRole;
            await _userRepository.UpdateAsync(user);

            return Results.Ok(new UpdateUserRoleResponse { Message = "Role updated successfully" });
        }
    }
}