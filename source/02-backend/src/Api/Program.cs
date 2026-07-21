using Synergistic.Application;
using Synergistic.Infrastructure;
<<<<<<< HEAD
=======
using Synergistic.Infrastructure.Persistence.Sql;
>>>>>>> dev-to-main

var builder = WebApplication.CreateBuilder(args);

// Register controllers — MVC pattern (ADR-009).
builder.Services.AddControllers();

// Register Application and Infrastructure layer services.
<<<<<<< HEAD
// ADR-001: These are currently placeholders with no active registrations.
=======
// ADR-001: Application and Infrastructure DI registrations are placeholders.
>>>>>>> dev-to-main
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

<<<<<<< HEAD
=======
// ── DbUp Migration Mode ────────────────────────────────────
// ADR-013: DbUp migration execution at API container entrypoint.
// When invoked with --migrate-only, run migrations and exit.
// The entrypoint.sh script passes this flag before starting the API.
if (args.Contains("--migrate-only"))
{
    var connectionString = builder.Configuration.GetConnectionString("Default");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        Console.Error.WriteLine("ERROR: ConnectionStrings__Default is not configured.");
        Environment.Exit(1);
    }

    var migrationsDir = Path.Combine(AppContext.BaseDirectory, "migrations");
    if (!Directory.Exists(migrationsDir))
    {
        Console.Error.WriteLine($"ERROR: Migrations directory not found: {migrationsDir}");
        Environment.Exit(1);
    }

    Console.WriteLine($"Running DbUp migrations from: {migrationsDir}");
    var result = DatabaseMigrator.RunMigrations(connectionString, migrationsDir);

    if (!result.Successful)
    {
        Console.Error.WriteLine($"ERROR: Migration failed: {result.ErrorMessage}");
        Environment.Exit(1);
    }

    Console.WriteLine($"Migrations complete. {result.ScriptsApplied} script(s) applied.");
    return; // Exit after migrations — do not start the web server
}

>>>>>>> dev-to-main
// Map controller routes — ASP.NET Core Controller pattern (ADR-009).
app.MapControllers();

app.Run();