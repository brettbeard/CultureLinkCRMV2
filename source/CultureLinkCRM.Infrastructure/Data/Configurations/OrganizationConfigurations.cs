using CultureLinkCRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CultureLinkCRM.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.Property(o => o.Name).IsRequired().HasMaxLength(300);
        builder.HasIndex(o => o.Name);
    }
}

public class OrganizationAddressConfiguration : IEntityTypeConfiguration<OrganizationAddress>
{
    public void Configure(EntityTypeBuilder<OrganizationAddress> builder)
    {
        builder.HasOne(a => a.Organization).WithMany(o => o.Addresses).HasForeignKey(a => a.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(a => a.Street1).IsRequired().HasMaxLength(300);
        builder.Property(a => a.City).IsRequired().HasMaxLength(150);
        builder.Property(a => a.StateProvince).HasMaxLength(150);
        builder.Property(a => a.PostalCode).HasMaxLength(30);
        builder.Property(a => a.Country).IsRequired().HasMaxLength(100);
    }
}

public class OrganizationPhoneConfiguration : IEntityTypeConfiguration<OrganizationPhone>
{
    public void Configure(EntityTypeBuilder<OrganizationPhone> builder)
    {
        builder.HasOne(p => p.Organization).WithMany(o => o.Phones).HasForeignKey(p => p.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(p => p.Number).IsRequired().HasMaxLength(30);
    }
}

public class OrganizationEmailConfiguration : IEntityTypeConfiguration<OrganizationEmail>
{
    public void Configure(EntityTypeBuilder<OrganizationEmail> builder)
    {
        builder.HasOne(e => e.Organization).WithMany(o => o.Emails).HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(e => e.Address).IsRequired().HasMaxLength(320);
    }
}

public class OrganizationNetworkConfiguration : IEntityTypeConfiguration<OrganizationNetwork>
{
    public void Configure(EntityTypeBuilder<OrganizationNetwork> builder)
    {
        builder.HasOne(l => l.Organization).WithMany(o => o.NetworkLinks).HasForeignKey(l => l.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(l => l.Network).WithMany(n => n.OrganizationLinks).HasForeignKey(l => l.NetworkId).OnDelete(DeleteBehavior.Restrict);
    }
}
