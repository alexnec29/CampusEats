using Microsoft.EntityFrameworkCore;
// --- ADAUGĂ ASTA: Namespace-ul unde se află entitățile tale ---
using CampusEats.Api.Domain.Entities; 

namespace CampusEats.Infrastructure;

public class CampusEatsDbContext : DbContext
{
    public CampusEatsDbContext(DbContextOptions<CampusEatsDbContext> options)
        : base(options) { }

    // --- ADAUGĂ ACESTE LINII ---
    // Acestea devin tabelele tale în baza de date
    
    public DbSet<User> Users { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Allergen> Allergens { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<KitchenTask> KitchenTasks { get; set; }
    public DbSet<LoyaltyAccount> LoyaltyAccounts { get; set; }
    // Adaugă aici orice altă entitate (ex: Payment, LoyaltyTransaction)
    
    // --- SFÂRȘITUL ADĂUGIRILOR ---
    
    // De asemenea, este o practică bună să adaugi și metoda OnModelCreating
    // pentru a configura relațiile dintre tabele (chei externe, indecși etc.)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Această linie va căuta automat toate fișierele de configurare
        // (IEntityTypeConfiguration) din proiect și le va aplica.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CampusEatsDbContext).Assembly);
    }
}