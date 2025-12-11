using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CampusEats.Api.Behaviors;
using CampusEats.Api.Endpoints;
using CampusEats.Api.Features.KitchenTask;
using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Infrastructure;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Middleware;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Utils.JwtUtil;
using CampusEats.Api.Utils.PaymentUtil;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5267")
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AllRoles", policy => policy.RequireRole(nameof(Role.Kitchen), nameof(Role.Buyer), nameof(Role.Admin)))
    .AddPolicy("Admin", policy => policy.RequireRole(nameof(Role.Admin)))
    .AddPolicy("Buyer", policy => policy.RequireRole(nameof(Role.Buyer), nameof(Role.Admin)))
    .AddPolicy("Kitchen", policy => policy.RequireRole(nameof(Role.Kitchen), nameof(Role.Admin)));


builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddDbContext<CampusEatsDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Payment Service
builder.Services.AddScoped<IPaymentService, StripePaymentService>();
builder.Services.AddScoped<IPaymentService, PayPalPaymentService>();
builder.Services.AddScoped<PaymentProviderFactory>();

// Jwt Service
builder.Services.AddScoped<IJwtService<User>, JwtService>();
builder.Services.AddSingleton<JwtSecurityTokenHandler>();

// Repositories
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IAllergenRepository, AllergenRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ILoyaltyAccountRepository, LoyaltyAccountRepository>();
builder.Services.AddScoped<ILoyaltyTransactionRepository, LoyaltyTransactionRepository>();
builder.Services.AddScoped<IKitchenTaskRepository, KitchenTaskRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBlackListTokenRepository, BlackListTokenRepository>();
builder.Services.AddScoped<IBuyerProfileRepository, BuyerProfileRepository>();
builder.Services.AddScoped<IKitchenProfileRepository, KitchenProfileRepository>();

builder.Services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(typeof(Program).Assembly); });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CampusEatsDbContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseCors();
    app.UseMiddleware<CsrfTokenFilterMiddleware>();
    app.UseMiddleware<JwtFilterMiddleware>();
    app.UseAuthorization();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.MapTestEndpoints();
app.MapUserEndpoints();
app.MapOrderEndpoints();
app.MapAllergenEndpoints();
app.MapKitchenEndpoints();
app.MapMenuItemEndpoints();
app.MapAdminEndpoints();
app.MapPaymentEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CampusEatsDbContext>();
    await DbInitializer.InitializeAsync(dbContext);
}

app.Run();