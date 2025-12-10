using System.Net;
using CampusEats.Api.Utils.CookieUtil;

namespace CampusEats.Api.Features.User;

public class LogoutUserResponse : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        var response = httpContext.Response;
        
        CookieService.DeleteJwtCookie(response);
        CookieService.DeleteCsrfCookie(response);
        
        response.StatusCode = StatusCodes.Status200OK;
        
        return response.WriteAsJsonAsync(new { success = true });
    }
}