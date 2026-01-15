using System.Security.Claims;
using CampusEats.Api.Infrastructure;
using CampusEats.Api.Models.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CampusEats.Test.Fixtures;

/// <summary>
/// WebApplicationFactory for CampusEats API testing with in-memory database
/// and test authentication/authorization setup
/// </summary>
public class CampusEatsWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestDatabaseName = "CampusEatsTestDb";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override configuration for tests
            var testConfig = new Dictionary<string, string?>
            {
                { "Stripe:SecretKey", "sk_test_fake_key_for_testing" },
                { "Stripe:PublishableKey", "pk_test_fake_key_for_testing" }
            };
            config.AddInMemoryCollection(testConfig);
        });
        
        builder.ConfigureServices(services =>
        {
            // Remove the production DbContext
            var dbContextDescriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<CampusEatsDbContext>));

            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            // Add in-memory database for testing
            services.AddDbContext<CampusEatsDbContext>(options =>
            {
                options.UseInMemoryDatabase(TestDatabaseName + Guid.NewGuid());
            });

            // Add test authentication handler
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<CampusEatsDbContext>();
            var logger = scopedServices
                .GetRequiredService<ILogger<CampusEatsWebApplicationFactory>>();

            // Ensure the database is created
            db.Database.EnsureCreated();

            try
            {
                // Seed the database with test data
                SeedTestDatabase(db);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the database with test messages. Error: {Message}", ex.Message);
            }
        });
    }

    private static void SeedTestDatabase(CampusEatsDbContext context)
    {
        // Seed test data if needed
        // This can be extended to add users, menu items, etc.
        if (!context.Users.Any())
        {
            // Add test users if needed
        }

        context.SaveChanges();
    }

    /// <summary>
    /// Creates an HttpClient with test authentication pre-configured
    /// </summary>
    public HttpClient CreateAuthenticatedClient(
        string userId = "00000000-0000-0000-0000-000000000001",
        string email = "test@example.com",
        Role role = Role.Buyer)
    {
        var client = CreateClient();

        var claims = new[]
        {
            new Claim("/id", userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId),
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer test-token");

        return client;
    }

    /// <summary>
    /// Creates a base HttpClient without authentication
    /// </summary>
    public HttpClient CreateUnauthenticatedClient()
    {
        return CreateClient();
    }
}
