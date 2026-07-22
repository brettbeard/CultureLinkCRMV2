using CultureLinkCRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CultureLinkCRM.Infrastructure.Data.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.MiddleName).HasMaxLength(200);
        builder.Property(p => p.Suffix).HasMaxLength(50);

        builder.HasOne(p => p.Household)
            .WithMany(h => h.Members)
            .HasForeignKey(p => p.HouseholdId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.LastName, p.FirstName });
    }
}

public class PersonAddressConfiguration : IEntityTypeConfiguration<PersonAddress>
{
    public void Configure(EntityTypeBuilder<PersonAddress> builder)
    {
        builder.HasOne(a => a.Person).WithMany(p => p.Addresses).HasForeignKey(a => a.PersonId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(a => a.Street1).IsRequired().HasMaxLength(300);
        builder.Property(a => a.City).IsRequired().HasMaxLength(150);
        builder.Property(a => a.StateProvince).HasMaxLength(150);
        builder.Property(a => a.PostalCode).HasMaxLength(30);
        builder.Property(a => a.Country).IsRequired().HasMaxLength(100);
    }
}

public class PersonPhoneConfiguration : IEntityTypeConfiguration<PersonPhone>
{
    public void Configure(EntityTypeBuilder<PersonPhone> builder)
    {
        builder.HasOne(p => p.Person).WithMany(p => p.Phones).HasForeignKey(p => p.PersonId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(p => p.Number).IsRequired().HasMaxLength(30);
    }
}

public class PersonEmailConfiguration : IEntityTypeConfiguration<PersonEmail>
{
    public void Configure(EntityTypeBuilder<PersonEmail> builder)
    {
        builder.HasOne(e => e.Person).WithMany(p => p.Emails).HasForeignKey(e => e.PersonId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(e => e.Address).IsRequired().HasMaxLength(320);
    }
}

public class PersonOrganizationConfiguration : IEntityTypeConfiguration<PersonOrganization>
{
    public void Configure(EntityTypeBuilder<PersonOrganization> builder)
    {
        builder.HasOne(l => l.Person).WithMany(p => p.OrganizationLinks).HasForeignKey(l => l.PersonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(l => l.Organization).WithMany(o => o.PersonLinks).HasForeignKey(l => l.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(l => l.RoleTitle).HasMaxLength(200);
    }
}

public class PersonNetworkConfiguration : IEntityTypeConfiguration<PersonNetwork>
{
    public void Configure(EntityTypeBuilder<PersonNetwork> builder)
    {
        builder.HasOne(l => l.Person).WithMany(p => p.NetworkLinks).HasForeignKey(l => l.PersonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(l => l.Network).WithMany(n => n.PersonLinks).HasForeignKey(l => l.NetworkId).OnDelete(DeleteBehavior.Restrict);
    }
}
