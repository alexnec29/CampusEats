using System.Text.Json.Serialization;
using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.API.Infrastructure;
using CampusEats.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using CampusEats.Api.Behaviors;
using MediatR;
using CampusEats.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddDbContext<CampusEatsDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

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

app.MapPost("/api/user/register", async (CreateUserRequest request, CreateUserHandler handler) =>
    await handler.Handle(request));
app.MapPost("/api/user/login", async (LoginUserRequest request, LoginUserHandler handler) =>
    await handler.Handle(request));

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