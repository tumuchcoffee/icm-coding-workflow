using Synergistic.Api.Endpoints.Health;
using Synergistic.Application;
using Synergistic.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Register Application and Infrastructure layer services.
// ADR-001: These are currently placeholders with no active registrations.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Map endpoints — Minimal API pattern (ADR-002).
app.MapHealthEndpoints();

app.Run();