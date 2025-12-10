using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CampusEats.Api.Models;

namespace CampusEats.Api.Infrastructure.EntityConfigurations;

public class LoyaltyAccountConfiguration : IEntityTypeConfiguration<LoyaltyAccount>
{
    public void Configure(EntityTypeBuilder<LoyaltyAccount> builder)
    {
        builder.HasKey(l => l.Id);

        builder.HasOne(l => l.User)
            .WithOne()
            .HasForeignKey<LoyaltyAccount>(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Transactions)
            .WithOne(t => t.LoyaltyAccount)
            .HasForeignKey(t => t.LoyaltyAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("LoyaltyAccounts");
    }
}