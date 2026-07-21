using Synergistic.Infrastructure.Persistence.Sql;

namespace Synergistic.Infrastructure.Tests.Persistence.Sql;

/// <summary>
/// Unit tests for <see cref="DatabaseMigrator"/>.
/// </summary>
public class DatabaseMigratorTests
{
    [Fact]
    public void RunMigrations_WithInvalidConnectionString_ReturnsFailure()
    {
        // Arrange
        var connectionString = "Server=nonexistent-host,1433;Database=Bogus;User Id=sa;Password=InvalidPass123!;TrustServerCertificate=True;Connect Timeout=5;";
        var migrationsDir = Path.Combine(Path.GetTempPath(), "migrations-empty");
        Directory.CreateDirectory(migrationsDir);

        try
        {
            // Act
            var result = DatabaseMigrator.RunMigrations(connectionString, migrationsDir);

            // Assert
            Assert.False(result.Successful);
            Assert.Equal(0, result.ScriptsApplied);
            Assert.NotNull(result.ErrorMessage);
        }
        finally
        {
            if (Directory.Exists(migrationsDir))
                Directory.Delete(migrationsDir, recursive: true);
        }
    }

    [Fact]
    public void RunMigrations_WithMissingDirectory_ThrowsExpectedError()
    {
        // Arrange
        var connectionString = "Server=nonexistent-host,1433;Database=Bogus;User Id=sa;Password=InvalidPass123!;TrustServerCertificate=True;Connect Timeout=5;";
        var migrationsDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            // Act
            var result = DatabaseMigrator.RunMigrations(connectionString, migrationsDir);

            // Assert
            Assert.False(result.Successful);
            Assert.NotNull(result.ErrorMessage);
        }
        finally
        {
            if (Directory.Exists(migrationsDir))
                Directory.Delete(migrationsDir, recursive: true);
        }
    }

    [Fact]
    public void MigrationResult_Failure_HasZeroScriptsApplied()
    {
        // Act
        var result = MigrationResult.Failure(null);

        // Assert
        Assert.False(result.Successful);
        Assert.Equal(0, result.ScriptsApplied);
        Assert.Equal("Unknown migration error.", result.ErrorMessage);
    }

    [Fact]
    public void MigrationResult_Success_HasCorrectCount()
    {
        // Act
        var result = MigrationResult.Success(5);

        // Assert
        Assert.True(result.Successful);
        Assert.Equal(5, result.ScriptsApplied);
        Assert.Null(result.ErrorMessage);
    }
}