using System.Reflection;
using DbUp;
using Microsoft.Extensions.Configuration;

namespace Synergistic.Infrastructure.Persistence.Sql;

/// <summary>
/// Executes idempotent DbUp migrations against the SQL Server database.
/// ADR-004: DbUp for versioned, idempotent SQL migrations.
/// ADR-013: Migration execution at API container entrypoint.
/// </summary>
public static class DatabaseMigrator
{
    /// <summary>
    /// Runs all pending SQL migration scripts from the specified directory.
    /// Returns a result indicating success or details of the failure.
    /// </summary>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="migrationsDirectory">The directory containing .sql migration scripts.</param>
    /// <returns>A <see cref="MigrationResult"/> indicating success or failure.</returns>
    public static MigrationResult RunMigrations(string connectionString, string migrationsDirectory)
    {
        EnsureDatabase.For.SqlDatabase(connectionString);

        var upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithScriptsFromFileSystem(migrationsDirectory)
            .JournalToSqlTable("dbo", "SchemaVersion")
            .WithTransactionPerScript()
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            return MigrationResult.Failure(result.Error);
        }

        return MigrationResult.Success(result.Scripts.Count());
    }
}

/// <summary>
/// Represents the outcome of a migration execution.
/// </summary>
/// <param name="Successful">Whether all migrations completed without error.</param>
/// <param name="ScriptsApplied">Number of scripts applied (0 if already up-to-date).</param>
/// <param name="ErrorMessage">Error details if migration failed; null otherwise.</param>
public sealed record MigrationResult(
    bool Successful,
    int ScriptsApplied,
    string? ErrorMessage = null
)
{
    public static MigrationResult Success(int scriptsApplied) =>
        new(true, scriptsApplied);

    public static MigrationResult Failure(Exception? error) =>
        new(false, 0, error?.ToString() ?? "Unknown migration error.");
}
