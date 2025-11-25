using System.Data.Common;
using CampusEats.Api.Utils.JwtUtil;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.User;

public class LogoutUserHandler(IJwtService<Models.User> jwtService) : IRequestHandler<LogoutUserRequest, IResult>
{
    public async Task<IResult> Handle(LogoutUserRequest request, CancellationToken cancellationToken)
    {
        await jwtService.BlackListToken(request.Jwt);
        return Results.Ok();
    }
}