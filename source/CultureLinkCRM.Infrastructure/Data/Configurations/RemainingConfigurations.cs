using CultureLinkCRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CultureLinkCRM.Infrastructure.Data.Configurations;

public class NetworkConfiguration : IEntityTypeConfiguration<Network>
{
    public void Configure(EntityTypeBuilder<Network> builder)
    {
        builder.Property(n => n.Name).IsRequired().HasMaxLength(300);
        builder.HasOne(n => n.ParentNetwork)
            .WithMany(n => n.ChildNetworks)
            .HasForeignKey(n => n.ParentNetworkId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SegmentConfiguration : IEntityTypeConfiguration<Segment>
{
    public void Configure(EntityTypeBuilder<Segment> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(300);
        builder.HasIndex(s => s.Name).IsUnique();

        builder.HasData(
            new Segment { Id = CultureLinkCRM.Core.SeedIds.DonorActiveSegmentId, Name = "Donor - Active", IsComputed = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), ModifiedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Segment { Id = CultureLinkCRM.Core.SeedIds.DonorLapsedSegmentId, Name = "Donor - Lapsed", IsComputed = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), ModifiedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}

public class SegmentAssignmentConfiguration : IEntityTypeConfiguration<SegmentAssignment>
{
    public void Configure(EntityTypeBuilder<SegmentAssignment> builder)
    {
        builder.HasOne(a => a.Segment).WithMany(s => s.Assignments).HasForeignKey(a => a.SegmentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.Person).WithMany(p => p.SegmentAssignments).HasForeignKey(a => a.PersonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.Organization).WithMany(o => o.SegmentAssignments).HasForeignKey(a => a.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class DonationConfiguration : IEntityTypeConfiguration<Donation>
{
    public void Configure(EntityTypeBuilder<Donation> builder)
    {
        builder.Property(d => d.Amount).HasColumnType("decimal(18,2)");
        builder.Property(d => d.FundProjectDesignation).HasMaxLength(300);
        builder.HasOne(d => d.Person).WithMany().HasForeignKey(d => d.PersonId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.Organization).WithMany().HasForeignKey(d => d.OrganizationId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.Property(s => s.Key).IsRequired().HasMaxLength(200);
        builder.HasIndex(s => s.Key).IsUnique();
        builder.Property(s => s.Value).IsRequired().HasMaxLength(1000);

        builder.HasData(new SystemSetting
        {
            Id = 1,
            Key = SystemSetting.LapsedDonorThresholdMonthsKey,
            Value = SystemSetting.DefaultLapsedDonorThresholdMonths
        });
    }
}

public class SeminarConfiguration : IEntityTypeConfiguration<Seminar>
{
    public void Configure(EntityTypeBuilder<Seminar> builder)
    {
        builder.Property(s => s.City).IsRequired().HasMaxLength(200);
        builder.HasOne(s => s.ParentSeminar)
            .WithMany(s => s.ChildSeminars)
            .HasForeignKey(s => s.ParentSeminarId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SeminarAttendanceConfiguration : IEntityTypeConfiguration<SeminarAttendance>
{
    public void Configure(EntityTypeBuilder<SeminarAttendance> builder)
    {
        builder.HasOne(a => a.Seminar).WithMany(s => s.Attendances).HasForeignKey(a => a.SeminarId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.Person).WithMany().HasForeignKey(a => a.PersonId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CurriculumOrderConfiguration : IEntityTypeConfiguration<CurriculumOrder>
{
    public void Configure(EntityTypeBuilder<CurriculumOrder> builder)
    {
        builder.HasOne(o => o.Person).WithMany().HasForeignKey(o => o.PersonId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.Organization).WithMany().HasForeignKey(o => o.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.LinkedOrganization).WithMany().HasForeignKey(o => o.LinkedOrganizationId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class EngagementTypeConfiguration : IEntityTypeConfiguration<EngagementType>
{
    public void Configure(EntityTypeBuilder<EngagementType> builder)
    {
        builder.Property(t => t.Name).IsRequired().HasMaxLength(150);
        builder.HasIndex(t => t.Name).IsUnique();

        builder.HasData(
            new EngagementType { Id = 1, Name = "Consulting" },
            new EngagementType { Id = 2, Name = "Coaching" },
            new EngagementType { Id = 3, Name = "STM Trip" },
            new EngagementType { Id = 4, Name = "International Partnership" }
        );
    }
}

public class EngagementConfiguration : IEntityTypeConfiguration<Engagement>
{
    public void Configure(EntityTypeBuilder<Engagement> builder)
    {
        builder.Property(e => e.Notes).HasMaxLength(4000);
        builder.HasOne(e => e.Person).WithMany().HasForeignKey(e => e.PersonId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Organization).WithMany().HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.EngagementType).WithMany(t => t.Engagements).HasForeignKey(e => e.EngagementTypeId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class AudienceConfiguration : IEntityTypeConfiguration<Audience>
{
    public void Configure(EntityTypeBuilder<Audience> builder)
    {
        builder.Property(a => a.Name).IsRequired().HasMaxLength(300);
    }
}

public class AudienceSegmentConfiguration : IEntityTypeConfiguration<AudienceSegment>
{
    public void Configure(EntityTypeBuilder<AudienceSegment> builder)
    {
        builder.HasOne(l => l.Audience).WithMany(a => a.SegmentLinks).HasForeignKey(l => l.AudienceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(l => l.Segment).WithMany(s => s.AudienceLinks).HasForeignKey(l => l.SegmentId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Email).IsRequired().HasMaxLength(320);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).IsRequired();
    }
}
