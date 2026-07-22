using CultureLinkCRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CultureLinkCRM.Infrastructure.Data.Configurations;

public class HouseholdConfiguration : IEntityTypeConfiguration<Household>
{
    public void Configure(EntityTypeBuilder<Household> builder)
    {
        builder.Property(h => h.HouseholdName).IsRequired().HasMaxLength(300);
        builder.HasIndex(h => h.HouseholdName);
    }
}

public class HouseholdAddressConfiguration : IEntityTypeConfiguration<HouseholdAddress>
{
    public void Configure(EntityTypeBuilder<HouseholdAddress> builder)
    {
        builder.HasOne(a => a.Household).WithMany(h => h.Addresses).HasForeignKey(a => a.HouseholdId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(a => a.Street1).IsRequired().HasMaxLength(300);
        builder.Property(a => a.City).IsRequired().HasMaxLength(150);
        builder.Property(a => a.StateProvince).HasMaxLength(150);
        builder.Property(a => a.PostalCode).HasMaxLength(30);
        builder.Property(a => a.Country).IsRequired().HasMaxLength(100);
    }
}

public class HouseholdPhoneConfiguration : IEntityTypeConfiguration<HouseholdPhone>
{
    public void Configure(EntityTypeBuilder<HouseholdPhone> builder)
    {
        builder.HasOne(p => p.Household).WithMany(h => h.Phones).HasForeignKey(p => p.HouseholdId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(p => p.Number).IsRequired().HasMaxLength(30);
    }
}

public class HouseholdEmailConfiguration : IEntityTypeConfiguration<HouseholdEmail>
{
    public void Configure(EntityTypeBuilder<HouseholdEmail> builder)
    {
        builder.HasOne(e => e.Household).WithMany(h => h.Emails).HasForeignKey(e => e.HouseholdId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(e => e.Address).IsRequired().HasMaxLength(320);
    }
}
