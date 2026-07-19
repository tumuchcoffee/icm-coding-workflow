namespace Synergistic.Application.Features.Health;

/// <summary>
/// Response model for the health check endpoint.
/// ADR-001: No MediatR — this is a plain record, not an IRequest.
/// ADR-002: Used by Minimal API endpoint group.
/// </summary>
public sealed record HealthCheckResponse(
    string Status,
    string Timestamp,
    string Version
);