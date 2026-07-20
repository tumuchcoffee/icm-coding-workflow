using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Synergistic.Infrastructure;

/// <summary>
/// Registers Infrastructure-layer services in the DI container.
/// ADR-001: Currently a placeholder — no Dapper, no external service clients.
/// ADR-004: When DbUp is integrated, the migration runner will be wired here.
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