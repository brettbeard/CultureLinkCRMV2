using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Infrastructure.Data;
using CultureLinkCRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CultureLinkCRM.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCultureLinkCrmInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default") ?? "Data Source=culturelinkcrm.db";
        services.AddDbContext<CultureLinkCrmDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<IHouseholdService, HouseholdService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<INetworkService, NetworkService>();
        services.AddScoped<ISegmentService, SegmentService>();
        services.AddScoped<IDonorStatusService, DonorStatusService>();
        services.AddScoped<IDonationService, DonationService>();
        services.AddScoped<ISeminarService, SeminarService>();
        services.AddScoped<ICurriculumOrderService, CurriculumOrderService>();
        services.AddScoped<IEngagementService, EngagementService>();
        services.AddScoped<IAudienceService, AudienceService>();
        services.AddScoped<IContactExportService, ContactExportService>();
        services.AddScoped<IExcelExportService, ExcelExportService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();

        return services;
    }
}
