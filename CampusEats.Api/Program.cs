using CampusEats.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using CampusEats.Api.Behaviors;
using MediatR;
using CampusEats.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddDbContext<CampusEatsDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.MapGet("/ping", () => "pong").WithName("Ping");

app.MapGet("/db-test", async (CampusEatsDbContext db) =>
{
    try
    {
        var allergensCount = await db.Allergens.CountAsync();
        var menuCount = await db.MenuItems.CountAsync();

        return Results.Ok(new
        {
            Message = "✅ DB and seed working",
            Allergens = allergensCount,
            MenuItems = menuCount
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"❌ Connection failed: {ex.Message}");
    }
});
app.Run();