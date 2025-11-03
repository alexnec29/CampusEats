using CampusEats.Client.Components;

var builder = WebApplication.CreateBuilder(args);

// === Blazor setup ===
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// === HTTP client către API ===
builder.Services.AddHttpClient("CampusEatsApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:5078"); // pune aici portul real din API
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