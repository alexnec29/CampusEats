using CampusEats.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using CampusEats.Api.Behaviors;
using MediatR;
using CampusEats.Api.Middleware;
using CampusEats.Api.Extensions;

using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddDbContext<CampusEatsDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IAllergenRepository, AllergenRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ILoyaltyAccountRepository, LoyaltyAccountRepository>();
builder.Services.AddScoped<ILoyaltyTransactionRepository, LoyaltyTransactionRepository>();
builder.Services.AddScoped<IKitchenTaskRepository, KitchenTaskRepository>();

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

app.MapTestEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CampusEatsDbContext>();
    await DbInitializer.InitializeAsync(dbContext);
}

app.Run();