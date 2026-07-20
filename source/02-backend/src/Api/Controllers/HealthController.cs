using Microsoft.AspNetCore.Mvc;
using Synergistic.Application.Features.Health;

namespace Synergistic.Api.Controllers;

/// <summary>
/// Health check controller for API liveness verification.
/// ADR-001: No MediatR — handler logic is inline. Will be refactored
///          when the first real business feature is added.
/// ADR-009: ASP.NET Core Controller pattern per updated system architecture.
/// FR-007: Returns status, timestamp, and version.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Returns the API health status, current UTC timestamp, and version string.
    /// </summary>
    /// <response code="200">The API is healthy and responding.</response>
    [HttpGet]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        var response = new HealthCheckResponse(
            Status: "Healthy",
            Timestamp: DateTimeOffset.UtcNow.ToString("o"),
            Version: "0.1.2"
        );
        return Ok(response);
    }
}