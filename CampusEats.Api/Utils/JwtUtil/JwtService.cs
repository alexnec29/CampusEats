using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace CampusEats.Api.Utils.JwtUtil;

public class JwtService(
    IConfiguration config,
    JwtSecurityTokenHandler jwtSecurityTokenHandler,
    IBlackListTokenRepository blackListTokenRepository
    ) : IJwtService<User>
{
    public string GenerateToken(User user)
    {
        var issuer = config["Jwt:Issuer"];
        var audience = config["Jwt:Audience"];
        var secret = config["Jwt:Secret"];
        var expires = DateTime.UtcNow.Add(TimeSpan.FromHours(1));

        var claims = new List<Claim>
        {
            new Claim("/id", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return jwtSecurityTokenHandler.WriteToken(token);
    }

    public async Task BlackListToken(string token)
    {
        DateTime expires = jwtSecurityTokenHandler.ReadJwtToken(token).ValidTo;
        Jwt jwt = new Jwt(token, expires);
        await blackListTokenRepository.AddAsync(jwt);
    }

    public async Task<bool> IsTokenBlacklisted(string token)
    {
        Jwt? jwt = await blackListTokenRepository.GetByTextAsync(token);
        return jwt != null;
    }
}