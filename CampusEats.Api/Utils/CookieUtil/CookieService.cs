namespace CampusEats.Api.Utils.CookieUtil;

public class CookieService
{
    private CookieService() {}
    public static void CreateJwtCookie(string jwt, HttpResponse response)
    {
        var jwtCookieOptions = new CookieOptions(GetBaseCookieConfiguration())
        {
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddHours(1),
        };
        response.Cookies.Append("JWT", jwt, jwtCookieOptions);
    }

    public static void CreateCsrfCookie(string jwt, HttpResponse response)
    {
        var csrfCookieOptions = new CookieOptions(GetBaseCookieConfiguration())
        {
            HttpOnly = false,
            Expires = DateTimeOffset.UtcNow.AddHours(1),
        };
        response.Cookies.Append("CSRF-TOKEN", jwt, csrfCookieOptions);
    }
    
    public static void DeleteJwtCookie(HttpResponse response)
    {
        var jwtCookieOptions = new CookieOptions(GetBaseCookieConfiguration())
        {
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddHours(-1),
        };
        response.Cookies.Append("JWT", "", jwtCookieOptions);
    }

    public static void DeleteCsrfCookie(HttpResponse response)
    {
        var csrfCookieOptions = new CookieOptions(GetBaseCookieConfiguration())
        {
            HttpOnly = false,
            Expires = DateTimeOffset.UtcNow.AddHours(-1),
        };
        response.Cookies.Append("CSRF-TOKEN", "", csrfCookieOptions);
    }

    private static CookieOptions GetBaseCookieConfiguration()
    {
        return new CookieOptions()
        {
            Secure = false, // set false only for non-HTTPS dev environments
            SameSite = SameSiteMode.Strict,
            Path = "/"
        };
    }
}