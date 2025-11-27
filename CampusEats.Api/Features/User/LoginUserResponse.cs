using CampusEats.Api.Utils.CookieUtil;

namespace CampusEats.Api.Features.User;

public class LoginUserResponse(string jwt) : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        var response = httpContext.Response;
        
        string csrfToken = Guid.NewGuid().ToString();
        
        CookieService.CreateJwtCookie(jwt, response);
        CookieService.CreateCsrfCookie(csrfToken, response);

        response.StatusCode = StatusCodes.Status200OK;
        
        return response.WriteAsJsonAsync(new { success = true });
    }
}