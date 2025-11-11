// --- Adaugă aceste using-uri în plus ---
using System.Reflection;
using CampusEats.Api.Features.KitchenTask; // Pentru MapKitchenEndpoints()
using CampusEats.Infrastructure;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
// ----------------------------------------

var builder = WebApplication.CreateBuilder(args);

// --- 1. Înregistrarea Serviciilor (Dependency Injection) ---

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Înregistrează DbContext-ul (Asta aveai deja)
builder.Services.AddDbContext<CampusEatsDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// *NOU: Adaugă MediatR* (caută toate Handlerele din proiect)
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// *NOU: Adaugă AutoMapper* (caută toate Profile-urile din proiect)
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// *NOU: Adaugă FluentValidation* (caută toți Validatorii din proiect)
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// *NOU: Adaugă CORS*
// Permite clientului Blazor (care rulează pe alt port) să facă request-uri
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        // Asigură-te că URL-ul din appsettings.json se potrivește cu cel al clientului tău Blazor
        policy.WithOrigins(builder.Configuration["BlazorClientUrl"] ?? "https://localhost:7123") 
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


var app = builder.Build();

// --- 2. Configurarea Pipeline-ului (Middleware) ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// *NOU: Folosește politica CORS* (trebuie să fie înainte de Map... Endpoints)
app.UseCors("AllowBlazorClient");

// Aici poți adăuga și middleware-ul de Global Error Handling (Task #33)
// app.UseMiddleware<GlobalErrorHandlingMiddleware>(); 


// --- 3. Maparea Endpoint-urilor ---

app.MapGet("/ping", () => "pong").WithName("Ping");

app.MapGet("/db-test", async (CampusEatsDbContext db) =>
    {
        try
        {
            await db.Database.OpenConnectionAsync();
            await db.Database.CloseConnectionAsync();
            return Results.Ok("✅ Connected to PostgreSQL successfully!");
        }
        catch (Exception ex)
        {
            return Results.Problem($"❌ Connection failed: {ex.Message}");
        }
    })
    .WithName("DbTest");

// *NOU: Mapează endpoint-urile pentru Kitchen*
// Aceasta este metoda de extensie pe care am definit-o în KitchenTaskEndpoints.cs
app.MapKitchenEndpoints();

// Când vei crea celelalte features, le vei adăuga și pe ele aici:
// app.MapOrderEndpoints();
// app.MapMenuEndpoints();
// app.MapUserEndpoints();


app.Run();