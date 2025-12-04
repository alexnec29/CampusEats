using CampusEats.Client.Components;
using CampusEats.Client.Services;
using Microsoft.JSInterop;

var builder = WebApplication.CreateBuilder(args);

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] 
                 ?? throw new InvalidOperationException("ApiBaseUrl not configured.");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<CsrfTokenHandler>();

builder.Services.AddScoped(sp => 
{
    var handler = new HttpClientHandler
    {
        CookieContainer = new System.Net.CookieContainer(),
        UseCookies = true
    };
    
    var csrfHandler = sp.GetRequiredService<CsrfTokenHandler>();
    csrfHandler.InnerHandler = handler;

    return new HttpClient(csrfHandler)
    {
        BaseAddress = new Uri("http://localhost:5078/")
    };
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();