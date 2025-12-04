using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.User;

public record CheckAuthRequest : IRequest<IResult>;

public class CheckAuthHandler(IHttpContextAccessor httpContextAccessor) : IRequestHandler<CheckAuthRequest, IResult>
{
    public Task<IResult> Handle(CheckAuthRequest request, CancellationToken cancellationToken)
    {
        var user = httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            var username = user.FindFirst(ClaimTypes.Name)?.Value;

            return Task.FromResult(Results.Ok(new 
            { 
                IsAuthenticated = true,
                UserId = userId,
                Role = role,
                Username = username
            }));
        }

        return Task.FromResult(Results.Ok(new { IsAuthenticated = false }));
    }
}