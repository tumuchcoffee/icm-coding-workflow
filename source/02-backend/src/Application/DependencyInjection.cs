using Microsoft.Extensions.DependencyInjection;

namespace Synergistic.Application;

/// <summary>
/// Registers Application-layer services in the DI container.
/// ADR-001: Currently a placeholder — no MediatR, no FluentValidation.
/// When business features arrive in future versions, this is where
/// MediatR, FluentValidation, and pipeline behaviors will be registered.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Future: services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        // Future: services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}