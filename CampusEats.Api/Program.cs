using System.Text.Json.Serialization;
using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Infrastructure;
using CampusEats.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using CampusEats.Api.Behaviors;
using MediatR;
using CampusEats.Api.Middleware;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddDbContext<CampusEatsDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Handlers
builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<LoginUserHandler>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CampusEatsDbContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.MapPost("/api/user/register", async (CreateUserRequest request, IMediator mediator) =>
    await mediator.Send(request));
app.MapPost("/api/user/login", async (LoginUserRequest request, IMediator mediator) =>
    await mediator.Send(request));

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

app.MapGet("/test-menu", async (IMenuItemRepository repo) =>
{
    var newItem = new MenuItem
    {
        Name = "Test Pizza",
        Price = 11.5m,
        Category = MenuCategory.Breakfast,
        IsAvailable = true
    };
    
    await repo.AddAsync(newItem);

    newItem.Price = 12.0m;
    await repo.UpdateAsync(newItem);

    var allItems = await repo.GetAllAsync();

    await repo.DeleteAsync(newItem.Id);

    return Results.Ok(new
    {
        Message = "Menu repository test completed",
        TotalMenuItems = allItems.Count
    });
});

app.Run();