using Synergistic.Application;
using Synergistic.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Register controllers — MVC pattern (ADR-009).
builder.Services.AddControllers();

// Register Application and Infrastructure layer services.
// ADR-001: These are currently placeholders with no active registrations.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Map controller routes — ASP.NET Core Controller pattern (ADR-009).
app.MapControllers();

app.Run();