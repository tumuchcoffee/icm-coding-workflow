-- 001_CreateSchemaVersion.sql
-- Creates the migration tracking table if it does not exist.
-- Idempotent: safe to run multiple times.
-- ADR-004: Used by DbUp to track applied migrations.
-- FR-008: Foundation for versioned schema management.

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SchemaVersion' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.SchemaVersion
    (
        Id          INT             NOT NULL IDENTITY(1,1),
        ScriptName  NVARCHAR(255)   NOT NULL,
        Applied     DATETIME2(7)    NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_SchemaVersion
            PRIMARY KEY CLUSTERED (Id),

        CONSTRAINT UQ_SchemaVersion_ScriptName
            UNIQUE (ScriptName)
    );

    -- Record this migration as applied so DbUp does not re-run it.
    INSERT INTO dbo.SchemaVersion (ScriptName)
    VALUES ('001_CreateSchemaVersion.sql');
END;