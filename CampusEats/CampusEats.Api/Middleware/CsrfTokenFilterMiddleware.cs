using System.Net;

namespace CampusEats.Api.Middleware;

public class CsrfTokenFilterMiddleware
{
    private readonly RequestDelegate _next;

    public CsrfTokenFilterMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        if (context.Request.Cookies["JWT"] != null)
        {
            string? csrfTokenFromCookie = context.Request.Cookies["CSRF-TOKEN"];
            string? csrfTokenFromHeader = context.Request.Headers["X-CSRF-TOKEN"];

            if (csrfTokenFromCookie == null || !string.Equals(csrfTokenFromCookie, csrfTokenFromHeader))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("CSRF validation failed");
                return;
            }
        }

        await _next(context);
    }
}