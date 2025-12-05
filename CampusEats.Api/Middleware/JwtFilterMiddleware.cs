using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Utils.JwtUtil;
using Microsoft.IdentityModel.Tokens;

namespace CampusEats.Api.Middleware;

public class JwtFilterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    
    public JwtFilterMiddleware(
        RequestDelegate next, 
        IConfiguration config,
        JwtSecurityTokenHandler jwtSecurityTokenHandler)
    {
        _next = next;
        _config = config;
        _jwtSecurityTokenHandler = jwtSecurityTokenHandler;
    }

    public async Task InvokeAsync(HttpContext context, IJwtService<User> jwtService)
    {
        string? jwt = context.Request.Cookies["JWT"];
        if (jwt != null)
        {
            if (await jwtService.IsTokenBlacklisted(jwt))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("JWT is blacklisted.");
                return;
            }
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _config["Jwt:Audience"],
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"] ?? string.Empty)),
            };

            try
            {
                context.User = _jwtSecurityTokenHandler.ValidateToken(jwt, validationParameters, out _);
            }
            catch (Exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Invalid jwt");
                return;
            }
        }
        
        await _next(context);
    }
}