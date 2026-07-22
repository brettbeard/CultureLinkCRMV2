using CultureLinkCRM.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CultureLinkCRM.Infrastructure.Data;

public class CultureLinkCrmDbContext(DbContextOptions<CultureLinkCrmDbContext> options) : DbContext(options)
{
    public DbSet<Person> People => Set<Person>();
    public DbSet<PersonAddress> PersonAddresses => Set<PersonAddress>();
    public DbSet<PersonPhone> PersonPhones => Set<PersonPhone>();
    public DbSet<PersonEmail> PersonEmails => Set<PersonEmail>();
    public DbSet<PersonOrganization> PersonOrganizations => Set<PersonOrganization>();
    public DbSet<PersonNetwork> PersonNetworks => Set<PersonNetwork>();

    public DbSet<Household> Households => Set<Household>();
    public DbSet<HouseholdAddress> HouseholdAddresses => Set<HouseholdAddress>();
    public DbSet<HouseholdPhone> HouseholdPhones => Set<HouseholdPhone>();
    public DbSet<HouseholdEmail> HouseholdEmails => Set<HouseholdEmail>();

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationAddress> OrganizationAddresses => Set<OrganizationAddress>();
    public DbSet<OrganizationPhone> OrganizationPhones => Set<OrganizationPhone>();
    public DbSet<OrganizationEmail> OrganizationEmails => Set<OrganizationEmail>();
    public DbSet<OrganizationNetwork> OrganizationNetworks => Set<OrganizationNetwork>();

    public DbSet<Network> Networks => Set<Network>();

    public DbSet<Segment> Segments => Set<Segment>();
    public DbSet<SegmentAssignment> SegmentAssignments => Set<SegmentAssignment>();

    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    public DbSet<Seminar> Seminars => Set<Seminar>();
    public DbSet<SeminarAttendance> SeminarAttendances => Set<SeminarAttendance>();

    public DbSet<CurriculumOrder> CurriculumOrders => Set<CurriculumOrder>();

    public DbSet<EngagementType> EngagementTypes => Set<EngagementType>();
    public DbSet<Engagement> Engagements => Set<Engagement>();

    public DbSet<Audience> Audiences => Set<Audience>();
    public DbSet<AudienceSegment> AudienceSegments => Set<AudienceSegment>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CultureLinkCrmDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        StampAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void StampAuditFields()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.ModifiedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = now;
            }
        }
    }
}
