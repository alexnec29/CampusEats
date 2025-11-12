using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CampusEats.Api.Models;

namespace CampusEats.Api.Infrastructure.EntityConfigurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.TotalAmount)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired();

        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Payment)
            .WithOne(p => p.Order)
            .HasForeignKey<Payment>(p => p.OrderId);

        builder.HasOne(o => o.KitchenTask)
            .WithOne(k => k.Order)
            .HasForeignKey<KitchenTask>(k => k.OrderId);

        builder.HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId);

        builder.ToTable("Orders");
    }
}