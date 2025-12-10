using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CampusEats.Api.Models;

namespace CampusEats.Api.Infrastructure.EntityConfigurations;

public class KitchenTaskConfiguration : IEntityTypeConfiguration<KitchenTask>
{
    public void Configure(EntityTypeBuilder<KitchenTask> builder)
    {
        builder.HasKey(k => k.Id);

        builder.HasOne(k => k.Order)
            .WithOne(o => o.KitchenTask)
            .HasForeignKey<KitchenTask>(k => k.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(k => k.AssignedStaff)
            .WithMany()
            .HasForeignKey(k => k.AssignedStaffId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable("KitchenTasks");
    }
}