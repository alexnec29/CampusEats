using Microsoft.EntityFrameworkCore;
using CampusEats.Api.Models;

namespace CampusEats.API.Infrastructure;

public class CampusEatsDbContext : DbContext
{
    public CampusEatsDbContext(DbContextOptions<CampusEatsDbContext> options)
        : base(options)
    {
        
    }
    public DbSet<User> Users { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; } = null!;
    public DbSet<Allergen> Allergens { get; set; } = null!;
}