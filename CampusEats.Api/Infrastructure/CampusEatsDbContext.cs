using Microsoft.EntityFrameworkCore;
using CampusEats.Api.Models;

namespace CampusEats.Api.Infrastructure;

public class CampusEatsDbContext : DbContext
{
    public CampusEatsDbContext(DbContextOptions<CampusEatsDbContext> options)
        : base(options) { }

    public DbSet<MenuItem> MenuItems { get; set; } = null!;
    public DbSet<Allergen> Allergens { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<MenuItemAllergen> MenuItemAllergens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CampusEatsDbContext).Assembly);
    }
}