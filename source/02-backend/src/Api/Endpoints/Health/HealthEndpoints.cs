using Synergistic.Application.Features.Health;

namespace Synergistic.Api.Endpoints.Health;

/// <summary>
/// Maps the health check endpoint for API liveness verification.
/// ADR-001: No MediatR — handler logic is inline. Will be refactored
///          when the first real business feature is added.
/// ADR-002: Minimal API endpoint group pattern per system architecture §4.5.
/// FR-007: Returns status, timestamp, and version.
/// </summary>
public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/health")
            .WithTags("Health");

        group.MapGet("/", () =>
        {
            var response = new HealthCheckResponse(
                Status: "Healthy",
                Timestamp: DateTimeOffset.UtcNow.ToString("o"),
                Version: "0.1.2"
            );
            return Results.Ok(response);
        })
        .Produces<HealthCheckResponse>(StatusCodes.Status200OK)
        .WithName("GetHealth")
        .WithDescription("Returns the API health status, current UTC timestamp, and version string.");
    }
}