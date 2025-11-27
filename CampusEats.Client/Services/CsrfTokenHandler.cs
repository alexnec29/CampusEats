using Microsoft.JSInterop;

namespace CampusEats.Client.Services;

public class CsrfTokenHandler : DelegatingHandler
{
    private readonly IJSRuntime _jsRuntime;

    public CsrfTokenHandler(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Only add CSRF token for non-GET requests
        if (request.Method != HttpMethod.Get)
        {
            try
            {
                // Attempt to get the token from the cookie
                var token = await _jsRuntime.InvokeAsync<string>("getCookie", cancellationToken, "XSRF-TOKEN");
                
                if (!string.IsNullOrEmpty(token))
                {
                    if (request.Headers.Contains("X-XSRF-TOKEN"))
                    {
                        request.Headers.Remove("X-XSRF-TOKEN");
                    }
                    request.Headers.Add("X-XSRF-TOKEN", token);
                }
            }
            catch (JSException)
            {
                
            }
            catch (InvalidOperationException)
            {
                
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
