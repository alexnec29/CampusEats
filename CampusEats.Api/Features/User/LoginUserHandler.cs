using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Utils.JwtUtil;
using MediatR;
using Microsoft.AspNetCore.Http; 

namespace CampusEats.Api.Features.User;

public class LoginUserHandler(
    IUserRepository userRepository, 
    IJwtService<Models.User> jwtService, 
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<LoginUserRequest, IResult>
{
    public async Task<IResult> Handle(LoginUserRequest request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByUsernameAsync(request.Username);
        if (user == null) return Results.NotFound("Username not found");

        if (request.Password != request.ConfirmPassword) 
            return Results.BadRequest("Passwords do not match");
            
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
            return Results.Unauthorized();
        
        string jwt = jwtService.GenerateToken(user);
        
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, 
            Secure = true,  
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddHours(24) 
        };

        httpContextAccessor.HttpContext?.Response.Cookies.Append("JWT", jwt, cookieOptions);
        
        return Results.Ok(new { Message = "Login successful" });
    }
}