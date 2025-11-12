using CampusEats.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusEats.Api.Infrastructure.EntityConfigurations
{
    public class AllergenConfiguration : IEntityTypeConfiguration<Allergen>
    {
        public void Configure(EntityTypeBuilder<Allergen> builder)
        {
            // Table name
            builder.ToTable("Allergens");

            // Primary key
            builder.HasKey(a => a.Id);

            // Properties
            builder.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.Description)
                .HasMaxLength(500);

            // Relationships
            builder.HasMany(a => a.MenuItemAllergens)
                .WithOne(ma => ma.Allergen)
                .HasForeignKey(ma => ma.AllergenId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}