using CampusEats.Api.Models;

namespace CampusEats.Api.Utils.JwtUtil;

public interface IJwtService<T>
{
    string GenerateToken(T entity);
}