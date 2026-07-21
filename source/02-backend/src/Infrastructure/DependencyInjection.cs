using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Synergistic.Infrastructure;

/// <summary>
/// Registers Infrastructure-layer services in the DI container.
/// ADR-001: Currently a placeholder for Dapper/data access registrations.
/// ADR-004: DbUp is invoked via CLI flag (--migrate-only), not through DI.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Future: services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
        // Future: services.AddScoped<ITenantRepository, TenantRepository>();
        return services;
    }
}