using CampusEats.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusEats.Api.Infrastructure.EntityConfigurations
{
    public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
    {
        public void Configure(EntityTypeBuilder<MenuItem> builder)
        {
            // Table name (optional)
            builder.ToTable("MenuItems");

            // Primary key
            builder.HasKey(m => m.Id);

            // Properties
            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.Description)
                .HasMaxLength(500);

            builder.Property(m => m.Price)
                .HasColumnType("decimal(10,2)") // precision for money
                .IsRequired();

            builder.Property(m => m.Category)
                .IsRequired();

            builder.Property(m => m.ImageUrl)
                .HasMaxLength(250);

            builder.Property(m => m.IsAvailable)
                .IsRequired();

            builder.Property(m => m.CreatedAt)
                .HasDefaultValueSql("NOW()");

            // Relationships
            builder.HasMany(m => m.MenuItemAllergens)
                .WithOne(ma => ma.MenuItem)
                .HasForeignKey(ma => ma.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(m => m.OrderItems)
                .WithOne(oi => oi.MenuItem)
                .HasForeignKey(oi => oi.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}