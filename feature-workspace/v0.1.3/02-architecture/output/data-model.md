# Data Model — Docker Local Development (v0.1.3)

**Feature**: Docker Local Development — Dockerfiles, Compose orchestration, and local containerized workflow
**Date**: 2026-07-20
**Version**: v0.1.3

---

## 1. Database Changes Overview

**v0.1.3 introduces no database schema changes.** The feature is purely infrastructure — Dockerfiles, a Compose file, and entrypoint scripts. The existing `dbo.SchemaVersion` table (created by migration `001_CreateSchemaVersion.sql` in v0.1.2) is the only database object and remains unchanged.

This document exists to confirm there are no data model changes and to document the database infrastructure within the Docker context.

---

## 2. Existing Schema (Unchanged)

### 2.1 dbo.SchemaVersion

| Column | SQL Type | Nullable | Constraint | Description |
|---|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY(1,1)`, `PRIMARY KEY CLUSTERED` | Auto-incrementing row identifier |
| `ScriptName` | `nvarchar(255)` | NOT NULL | `UNIQUE` | Fully qualified migration script filename |
| `Applied` | `datetime2(7)` | NOT NULL | `DEFAULT SYSUTCDATETIME()` | UTC timestamp of migration application |

**Source Migration:** `source/03-sql/migrations/001_CreateSchemaVersion.sql`

**v0.1.3 Impact:** None. The table is used by DbUp on API container startup (ADR-013) to determine which migrations are pending.

---

## 3. Database Connection Configuration

### 3.1 Docker Compose Connection String

The connection string is injected via the Docker Compose environment and differs from the v0.1.2 LocalDB connection:

| Environment | v0.1.2 (LocalDB) | v0.1.3 (Docker) |
|---|---|---|
| **Server** | `(LocalDB)\MSSQLLocalDB` | `icm-db,1433` |
| **Database** | `Synergistic` | `Synergistic` |
| **Auth** | Windows Integrated | SQL Server (SA) |
| **Trust Certificate** | N/A | `TrustServerCertificate=True` |

**Rationale:** The `TrustServerCertificate=True` flag is required because the SQL Server container uses a self-signed certificate by default. In production (Azure SQL Database), this is not needed — the certificate chain is trusted. This is a local development convenience only.

### 3.2 Connection String Template

```
Server=icm-db,1433;Database=Synergistic;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=True;
```

- `icm-db` resolves via Docker DNS to the SQL Server container (ADR-014)
- `1433` is the default SQL Server port
- `sa` is the SQL Server system administrator account (acceptable for local dev; production uses Managed Identity)
- `${MSSQL_SA_PASSWORD}` is substituted at container startup from the Compose environment or `.env` file

---

## 4. Migration Strategy (Unchanged from v0.1.2)

### 4.1 Execution Flow

```
┌──────────────────────────────────────────────────────┐
│              API Container Starts                      │
└─────────────────────┬────────────────────────────────┘
                      ▼
┌──────────────────────────────────────────────────────┐
│  entrypoint.sh: Wait for SQL Server (max 60 sec)     │
│  Poll: sqlcmd -S icm-db ... -Q "SELECT 1"            │
└─────────────────────┬────────────────────────────────┘
                      ▼ (SQL Server ready)
┌──────────────────────────────────────────────────────┐
│  entrypoint.sh: Run DbUp migrations                  │
│  dotnet Api.dll --migrate-only                       │
└─────────────────────┬────────────────────────────────┘
                      ▼
┌──────────────────────────────────────────────────────┐
│  DbUp: Query dbo.SchemaVersion for applied scripts   │
└─────────────────────┬────────────────────────────────┘
                      ▼
              ┌───────┴───────┐
              │ Pending?       │
              └───────┬───────┘
          Yes         │         No
          ▼           │         ▼
┌──────────────────┐  │  ┌──────────────────┐
│ Run pending      │  │  │ Skip — database  │
│ scripts in order │  │  │ is up to date    │
│ (transactional)  │  │  │                  │
└────────┬─────────┘  │  └──────────────────┘
         ▼            │
┌──────────────────┐  │
│ INSERT INTO      │  │
│ SchemaVersion    │  │
└────────┬─────────┘  │
         │            │
         ▼            ▼
┌──────────────────────────────────────────────────────┐
│  entrypoint.sh: Start API                            │
│  exec dotnet Api.dll                                 │
└──────────────────────────────────────────────────────┘
```

### 4.2 Migration Versioning Convention (Unchanged)

```
{sequenceNumber}_{Description}.sql
```

| Migration | Status | Description |
|---|---|---|
| `001_CreateSchemaVersion.sql` | Applied (v0.1.2) | Creates the migration tracking table |
| Future migrations | Pending | To be added in future feature versions |

### 4.3 Idempotency Rules (Unchanged)

Every migration script MUST:
1. Check for object existence before creating (`IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = '...')`)
2. Be safe to run multiple times without error or data loss
3. Never use `DROP` without a corresponding `CREATE` guarded by existence checks

---

## 5. Data Persistence in Docker

### 5.1 Named Volume Strategy

| Volume | Mount Point | Purpose | Lifecycle |
|---|---|---|---|
| `icm-sql-data` | `/var/opt/mssql/data` | SQL Server data files (`.mdf`, `.ldf`) | Persists across `docker compose down`; removed only by `docker compose down -v` |

**Rationale:** The named volume ensures that database changes (migrations applied, test data created) survive container restarts and `docker compose down`. This is critical for developer productivity — re-applying migrations and re-creating test data on every `docker compose up` would be slow and frustrating.

### 5.2 Volume Behavior Matrix

| Command | Database State After |
|---|---|
| `docker compose up` (first time) | Fresh database created; migrations applied |
| `docker compose down` then `up` | Database preserved; no migrations re-applied (idempotent) |
| `docker compose down -v` then `up` | Database wiped; migrations re-applied from scratch |
| `docker compose restart` | Database preserved; no migrations run |

---

## 6. Tenant Partitioning Strategy

**Not applicable for v0.1.3.** The existing `dbo.SchemaVersion` table is a system/infrastructure table with no tenant data. No new entity tables are introduced in this version.

When tenant support is introduced (future version, after authentication is implemented):
- Every entity table will include a `TenantId uniqueidentifier NOT NULL` column
- A non-clustered index on `TenantId` will be standard on every entity table
- Row-Level Security (RLS) predicates will enforce `TenantId` isolation at the database level
- The local Docker environment will use a synthetic tenant ID for development purposes

---

## 7. Environment Parity

| Aspect | Local (Docker) | Production (Azure) |
|---|---|---|
| **Database Engine** | SQL Server 2022 Developer Edition | Azure SQL Database |
| **Server Address** | `icm-db,1433` (Docker DNS) | `icm-prd-sql.database.windows.net` |
| **Authentication** | SQL Server (SA login) | Managed Identity + Azure AD |
| **TLS** | Self-signed (`TrustServerCertificate=True`) | Azure-issued certificate (trusted) |
| **Backup** | None (Docker volume) | Automated backups (7–35 day retention) |
| **High Availability** | Single container | Active geo-replication |
| **Connection String** | In Compose environment | Azure Key Vault reference |

**Principle**: **Consistency** — the local Docker environment uses the closest available containerized match to Azure SQL Database (SQL Server 2022 Developer Edition, ADR-016). The differences are in operational concerns (auth, TLS, HA) that are not relevant to local development.

---

*No schema changes in v0.1.3. This document confirms the data layer is unchanged and documents the Docker-specific database infrastructure.*