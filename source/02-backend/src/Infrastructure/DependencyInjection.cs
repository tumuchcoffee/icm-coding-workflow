using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Synergistic.Infrastructure;

/// <summary>
/// Registers Infrastructure-layer services in the DI container.
<<<<<<< HEAD
/// ADR-001: Currently a placeholder — no Dapper, no external service clients.
/// ADR-004: When DbUp is integrated, the migration runner will be wired here.
=======
/// ADR-001: Currently a placeholder for Dapper/data access registrations.
/// ADR-004: DbUp is invoked via CLI flag (--migrate-only), not through DI.
>>>>>>> dev-to-main
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