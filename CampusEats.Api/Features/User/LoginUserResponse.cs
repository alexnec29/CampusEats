namespace CampusEats.Api.Features.User;

public class LoginUserResponse(string jwt) : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        var response = httpContext.Response;
        
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // set false only for non-HTTPS dev environments
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(1),
            Path = "/"
        };

        string csrfToken = Guid.NewGuid().ToString();

        response.Cookies.Append("JWT", jwt, cookieOptions);
        response.Cookies.Append("CSRF-TOKEN", csrfToken, cookieOptions);
        response.StatusCode = StatusCodes.Status200OK;
        
        return response.WriteAsJsonAsync(new { success = true });
    }
}