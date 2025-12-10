using CampusEats.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusEats.Api.Infrastructure.EntityConfigurations
{
    public class MenuItemAllergenConfiguration : IEntityTypeConfiguration<MenuItemAllergen>
    {
        public void Configure(EntityTypeBuilder<MenuItemAllergen> builder)
        {
            // Table name
            builder.ToTable("MenuItemAllergens");

            // Composite primary key
            builder.HasKey(ma => new { ma.MenuItemId, ma.AllergenId });

            // Relationships
            builder.HasOne(ma => ma.MenuItem)
                .WithMany(m => m.MenuItemAllergens)
                .HasForeignKey(ma => ma.MenuItemId);

            builder.HasOne(ma => ma.Allergen)
                .WithMany(a => a.MenuItemAllergens)
                .HasForeignKey(ma => ma.AllergenId);
        }
    }
}