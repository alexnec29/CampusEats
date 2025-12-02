using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CampusEats.Api.Models;

namespace CampusEats.Api.Infrastructure.EntityConfigurations;

public class LoyaltyTransactionConfiguration : IEntityTypeConfiguration<LoyaltyTransaction>
{
    public void Configure(EntityTypeBuilder<LoyaltyTransaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TransactionType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(255);

        builder.ToTable("LoyaltyTransactions");
    }
}