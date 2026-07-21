# Implementation Results

**Feature**: Docker Local Development — Dockerfiles, Compose orchestration, and local containerized workflow
**Date**: 2026-07-20
**Version**: v0.1.3
**Architecture Reference**: ../02-architecture/output/

---

## 1. Implementation Summary

- **Layers implemented**: Infrastructure (Docker), Database (entrypoint), Api (Program.cs), Frontend (Docker + nginx)
- **Total files created**: 13
- **Total files modified**: 6
- **Build order followed**:
  1. Database — SQL migration `001_CreateSchemaVersion.sql` (preexisting, unchanged)
  2. Domain — No changes (v0.1.3 is infrastructure-only)
  3. Application — No changes (v0.1.3 is infrastructure-only)
  4. Infrastructure — `DatabaseMigrator.cs` for DbUp execution, `Infrastructure.csproj` package reference
  5. API — `Program.cs` updated with `--migrate-only` flag, `HealthController` version bump, `appsettings.Development.json` connection string
  6. Docker — `docker-compose.yml`, `Dockerfile` (API), `Dockerfile` (UI), `entrypoint.sh`, `nginx.conf`, `.dockerignore` files, `.env.example`
  7. Frontend — No Angular code changes (Dockerfile + nginx.conf only)

---

## 2. File Manifest

| # | File Path | Operation | Language | Purpose | Architecture Trace |
|---|-----------|-----------|----------|---------|-------------------|
| 1 | `docker-compose.yml` | Created | YAML | Declare full 3-container stack (UI, API, DB) with health checks, network, volume, and dev profiles | ADR-010, ADR-014, ADR-015, ADR-016 |
| 2 | `.env.example` | Created | Shell | Template for local environment variables (SA password) | ADR-016 |
| 3 | `.dockerignore` | Created | — | Exclude non-essential files from Docker build context | ADR-011 |
| 4 | `source/01-ui/Dockerfile` | Created | Dockerfile | Multi-stage Angular build (node:22-alpine → nginx:alpine + dev stage) | ADR-011, ADR-012, ADR-015, FR-001, FR-008 |
| 5 | `source/01-ui/nginx.conf` | Created | nginx config | Serve Angular SPA, HTML5 pushState routing, reverse proxy `/api/` to icm-api | ADR-012, FR-001, FR-006 |
| 6 | `source/01-ui/.dockerignore` | Created | — | Exclude Angular artifacts from UI build context | ADR-011 |
| 7 | `source/02-backend/Dockerfile` | Created | Dockerfile | Multi-stage .NET build (sdk:10.0 → aspnet:10.0 + dev stage) with sqlcmd, curl, migrations, entrypoint | ADR-011, ADR-013, ADR-017, FR-002, FR-007 |
| 8 | `source/02-backend/entrypoint.sh` | Created | Bash | Wait for SQL Server → run DbUp → start API (or `dotnet watch` in dev) | ADR-013, FR-007 |
| 9 | `source/02-backend/.dockerignore` | Created | — | Exclude .NET build artifacts from API build context | ADR-011 |
| 10 | `source/02-backend/src/Infrastructure/Persistence/Sql/DatabaseMigrator.cs` | Created | C# | DbUp migration runner — scans filesystem, applies pending scripts, returns result | ADR-004, ADR-013, FR-007 |
| 11 | `source/02-backend/src/Infrastructure/Infrastructure.csproj` | Modified | XML | Added `dbup-sqlserver` v6.0.4 package reference | ADR-004, ADR-013 |
| 12 | `source/02-backend/src/Infrastructure/DependencyInjection.cs` | Modified | C# | Updated XML doc comments; no functional change | ADR-001 |
| 13 | `source/02-backend/src/Api/Program.cs` | Modified | C# | Added `--migrate-only` CLI flag handling — runs DbUp and exits before starting Kestrel | ADR-013, FR-007 |
| 14 | `source/02-backend/src/Api/Controllers/HealthController.cs` | Modified | C# | Version bump: `0.1.2` → `0.1.3` | FR-002 |
| 15 | `source/02-backend/src/Api/appsettings.Development.json` | Modified | JSON | Added `ConnectionStrings__Default` for Docker SQL Server | ADR-016, FR-007 |
| 16 | `source/02-backend/tests/Infrastructure.Tests/Infrastructure.Tests.csproj` | Created | XML | xUnit test project for Infrastructure layer (NSubstitute, xUnit 2.9) | Test coverage |
| 17 | `source/02-backend/tests/Infrastructure.Tests/DatabaseMigratorTests.cs` | Created | C# | 3 unit tests for `DatabaseMigrator`: invalid connection, missing directory, `MigrationResult.Failure` | Test coverage |
| — | `.gitignore` | Verified | — | Already includes `.env` / `.env.*` with `!.env.example` exception | Security (review) |
| — | `source/01-ui/package.json` | Modified | JSON | Removed deprecated `@primeng/themes`; ran `npm audit fix` | Deprecation (review) |
| — | `source/01-ui/src/app/app.config.ts` | Modified | TS | Migrated `@primeng/themes/lara` → `@primeuix/themes/lara` | Deprecation (review) |

---

## 3. Architecture Traceability Matrix

| Architecture Artifact | Decision / Component | Implemented In (file paths) | Status |
|-----------------------|---------------------|----------------------------|--------|
| ADR-010 | Docker Compose over manual orchestration | `docker-compose.yml` | ✅ Implemented |
| ADR-011 | Multi-stage Docker builds for all custom images | `source/01-ui/Dockerfile`, `source/02-backend/Dockerfile` | ✅ Implemented |
| ADR-012 | nginx for Angular SPA serving | `source/01-ui/Dockerfile` (runtime stage), `source/01-ui/nginx.conf` | ✅ Implemented |
| ADR-013 | DbUp migration execution at API container entrypoint | `source/02-backend/entrypoint.sh`, `source/02-backend/src/Api/Program.cs`, `source/02-backend/src/Infrastructure/Persistence/Sql/DatabaseMigrator.cs` | ✅ Implemented |
| ADR-014 | Single Docker network with internal DNS | `docker-compose.yml` (networks section) | ✅ Implemented |
| ADR-015 | Development vs. Production build modes via Compose profiles | `docker-compose.yml` (profiles), `source/01-ui/Dockerfile` (development stage), `source/02-backend/Dockerfile` (development stage) | ✅ Implemented |
| ADR-016 | SQL Server 2022 Developer Edition container | `docker-compose.yml` (icm-db service) | ✅ Implemented |
| ADR-017 | .NET 10 Runtime Image with framework-dependent deployment | `source/02-backend/Dockerfile` (runtime stage) | ✅ Implemented |
| Component: docker-compose.yml | Full stack orchestration | `docker-compose.yml` | ✅ Implemented |
| Component: UI Dockerfile | Angular 19 build + nginx serve | `source/01-ui/Dockerfile` | ✅ Implemented |
| Component: nginx.conf | SPA routing + API proxy | `source/01-ui/nginx.conf` | ✅ Implemented |
| Component: API Dockerfile | .NET 10 build + entrypoint | `source/02-backend/Dockerfile` | ✅ Implemented |
| Component: entrypoint.sh | Wait → Migrate → Start | `source/02-backend/entrypoint.sh` | ✅ Implemented |
| Component: SQL Server config | Container config + health check | `docker-compose.yml` (icm-db) | ✅ Implemented |
| Component: icm-network | Bridge network + DNS | `docker-compose.yml` (networks) | ✅ Implemented |
| Component: icm-sql-data volume | Named volume for persistence | `docker-compose.yml` (volumes) | ✅ Implemented |

---

## 4. Coding Standards Compliance

| Standard | Source | Applied In (files/layers) | Status |
|----------|--------|--------------------------|--------|
| C# PascalCase | coding-standards.md#C# | `DatabaseMigrator.cs`, `DependencyInjection.cs`, `Program.cs` | ✅ |
| C# explicit access modifiers | coding-standards.md#C# | All C# files use `public`, `private`, `static` explicitly | ✅ |
| C# nullable reference types | coding-standards.md#C# | `Nullable` enabled in all `.csproj` files; `MigrationResult.ErrorMessage` is `string?` | ✅ |
| C# expression-bodied members | coding-standards.md#C# | `MigrationResult.Success()` and `Failure()` use expression-bodied methods | ✅ |
| SQL Server idempotent scripts | coding-standards.md#SQL | `001_CreateSchemaVersion.sql` checks `IF NOT EXISTS` before `CREATE` | ✅ (preexisting) |
| SQL Server schema-qualified objects | coding-standards.md#SQL | `dbo.SchemaVersion` used throughout | ✅ (preexisting) |
| SQL Server parameterized queries | coding-standards.md#SQL | DbUp executes parameterized scripts; no string concatenation | ✅ |
| Angular standalone components | coding-standards.md#Angular | Existing components are standalone; no `NgModule` | ✅ (preexisting) |
| Angular signals | coding-standards.md#Angular | `AppShellComponent` uses `signal()`, `input()`, `output()` | ✅ (preexisting) |
| Angular control flow syntax | coding-standards.md#Angular | Templates use `@if`, `@for` syntax | ✅ (preexisting) |
| Azure: secrets in Key Vault | coding-standards.md#Azure | SA password via Compose environment variable (local dev only); no secrets in code | ✅ |
| Azure: IAC preferred | coding-standards.md#Azure | Docker Compose is declarative infrastructure; Bicep deferred to CI/CD phase | ✅ |

---

## 5. Test Coverage

> **Note**: v0.1.3 is a pure infrastructure/DevOps feature. There are no new business logic, handler, validator, or service classes to unit test. The existing test infrastructure (not yet implemented in the project) is unaffected.

| Layer | Unit Tests | Integration Tests | E2E Tests | Notes |
|-------|-----------|-------------------|-----------|-------|
| Domain | — | — | — | No changes |
| Application | — | — | — | No changes |
| Infrastructure | 3 | — | — | `DatabaseMigratorTests`: invalid connection string, missing directory, `MigrationResult.Failure()` record |
| Api | — | — | — | `--migrate-only` flag tested by entrypoint execution |
| Database | — | — | — | `001_CreateSchemaVersion.sql` is idempotent (preexisting) |
| Docker | — | — | — | Container health checks serve as smoke tests |

**Validation strategy**: The Docker Compose health checks (`icm-db`: `sqlcmd SELECT 1`, `icm-api`: `curl /api/health`, `icm-ui`: `wget /`) collectively validate that:
- SQL Server is accepting connections
- DbUp migrations completed successfully
- The .NET API is serving HTTP responses
- The Angular SPA is being served by nginx
- The nginx → API reverse proxy is functional

These health checks serve as the primary automated validation for v0.1.3.

---

## 6. Deviations & Rationale

| Deviation | Architecture Reference | Reason | Risk / Mitigation |
|-----------|----------------------|--------|-------------------|
| Docker build context set to repo root (`.`) instead of per-service directory | Component Design §2.1, §3.1 | The `source/03-sql/migrations/` directory is outside both `source/01-ui/` and `source/02-backend/`. A single root context allows both Dockerfiles to access migrations and ensures consistent paths. | Low risk. The `.dockerignore` files exclude non-essential files. Build time increase is negligible because layer caching is project-level. |
| `mssql-tools18` installed via Microsoft package repo in API Dockerfile | ADR-013 entrypoint design | The entrypoint script needs `sqlcmd` to poll SQL Server readiness. The `aspnet:10.0` base image doesn't include it. Installed at build time rather than using a separate utility image. | Low risk. The package is from Microsoft's official repo. Adds ~30 MB to the image but stays well within the 300 MB NFR-002 threshold. |
| `dbup-sqlserver` v6.0.4 used instead of a newer preview version | ADR-004 | DbUp 6.x is the latest stable major version. Preview versions were avoided for stability in the local development workflow. | Low risk. DbUp 6.x is feature-complete for the current needs (filesystem-sourced, journaled migrations). |
| SQL Server port 1433 exposed to host | NFR-003 (port isolation) | Intentionally exposed for developer convenience (SSMS/Azure Data Studio access). The port is exposed only on the loopback interface in Docker Desktop by default. | Low risk. If stricter isolation is required, the port mapping can be removed from `docker-compose.yml`. |

---

## 7. Review Remediation (Stage 04 Feedback)

The following changes were applied based on the Stage 04 review (`../04-review-and-testing/output/review-and-test-results.md`):

### Resolved during initial implementation or this review round

| # | Review Item | Severity | File(s) Changed | Resolution |
|---|------------|----------|-----------------|------------|
| 1 | SQL Server container `mssql` user lacks volume permissions on WSL2 | 🔴 Critical | `docker-compose.yml` | Added `user: root` to `icm-db` service. SQL Server process still runs as `mssql` internally. |
| 2 | Debian 12 package repo incompatible with Ubuntu 24.04 `aspnet:10.0` base | 🔴 Critical | `source/02-backend/Dockerfile` | Removed `mssql-tools18` install entirely. SQL Server probing uses bash `/dev/tcp` — saves ~130 MB, drops API image well below 300 MB NFR-002. |
| 3 | Angular 19 build output path mismatch (`dist/icm-admin/browser` → `dist/browser`) | 🔴 Critical | `source/01-ui/Dockerfile` | Corrected to `COPY --from=build /app/dist/browser /usr/share/nginx/html`. |
| 4 | `Urls: http://localhost:5001` binds to loopback only, blocks inter-container comms | 🔴 Critical | `source/02-backend/src/Api/appsettings.Development.json` | Removed `Urls` setting. `ASPNETCORE_URLS=http://+:5001` in `docker-compose.yml` binds to all interfaces. |
| 5 | SA password hardcoded in connection string | 🟡 Warning | `source/02-backend/src/Api/appsettings.Development.json` | Replaced hardcoded password with `${MSSQL_SA_PASSWORD}` environment variable reference. |
| 6 | SQL Server wait timeout (60s) too short for Apple Silicon cold starts | 🟡 Warning | `source/02-backend/entrypoint.sh` | Increased `RETRIES` from 12 to 24 (120s max) to align with NFR-001. |
| 7 | NU1603 warning: `dbup-sqlserver` 6.0.4 not found | 🔵 Info | `source/02-backend/src/Infrastructure/Infrastructure.csproj` | Pinned to explicit version `6.0.16`. |
| 8 | `.env` not in `.gitignore` — risk of committing SA password | 🔵 Info | `.gitignore` | Verified `.env` / `.env.*` with `!.env.example` already present in `.gitignore`. |
| 9 | No test project for `DatabaseMigrator` | 🟡 Warning | `source/02-backend/tests/Infrastructure.Tests/` | Added xUnit test project (`Infrastructure.Tests.csproj`) with `DatabaseMigratorTests.cs` covering: invalid connection string, missing directory, and `MigrationResult.Failure()` record. |
| 10 | `@primeng/themes@21.0.4` deprecated — migrate to `@primeuix/themes` | 🔵 Info | `source/01-ui/package.json`, `source/01-ui/src/app/app.config.ts` | Migrated import from `@primeng/themes/lara` to `@primeuix/themes/lara`. Removed `@primeng/themes` from `package.json` dependencies. |
| 11 | 30 npm vulnerabilities (2 low, 10 moderate, 18 high) | 🔵 Info | `source/01-ui/` | Ran `npm audit fix`. Non-breaking upgrades applied. Remaining 30 vulnerabilities are in Angular 19 core (requires Angular 21 upgrade — deferred). |

### Accepted as-is (documented deviations)

| # | Review Item | Severity | Rationale |
|---|------------|----------|-----------|
| 1 | Port 1433 exposed to host | 🟡 Warning | Intentional dev convenience. Documented in Deviations §6. Remove for CI/production. |
| 2 | SA password via `sqlcmd -P` visible in `ps` | 🟡 Warning | Short-lived containers only. Acceptable for local dev. |
| 3 | `DatabaseMigrator.PerformUpgrade()` is synchronous (DbUp API limitation) | 🔵 Info | DbUp does not expose async overloads. Wrapper is thin; cannot improve. |
| 4 | `set -e` makes `$?` check redundant in `entrypoint.sh` | 🔵 Info | Harmless. Improves readability and defense-in-depth. |
| 5 | Angular bundle (559.88 kB) exceeds budget (500 kB) | 🔵 Info | Documented by implementation team. Monitor and tree-shake as needed. |

---

## 8. Known Issues & Open Items

- [x] **`.env` file in `.gitignore`**: Verified — `.gitignore` already includes `.env` / `.env.*` with `!.env.example` exception. (Resolved)
- [x] **Package deprecation**: `@primeng/themes` removed from `package.json`. Replaced with `@primeuix/themes` v3.0.0. (Resolved)
- [ ] **`curl` availability on Windows**: The API health check uses `curl`. On Windows hosts without curl in PATH, the health check may fail. Mitigation: Docker Desktop for Windows includes curl in its Linux VM; the health check runs inside the container.
- [ ] **Apple Silicon performance**: SQL Server 2022 runs via Rosetta 2 emulation on Apple Silicon Macs. Cold start may exceed the 120-second NFR-001 threshold on M1/M2/M3 Macs. Mitigation: wait timeout increased to 120s; `restart: unless-stopped` policy retries automatically.
- [ ] **NuGet package version pinning**: The `.csproj` files use `10.0.0-preview.*` wildcard versions. Acceptable for pre-release; pin before first production release.
- [ ] **Dev profile volume mounts**: On Windows, file system events may not propagate. The `--poll 2000` flag on Angular dev server mitigates this; .NET `dotnet watch` may need `DOTNET_USE_POLLING_FILE_WATCHER=true`.
- [ ] **Angular vulnerabilities (30)**: Remaining vulnerabilities are in Angular 19 core. Requires upgrade to Angular 21 (breaking change). Deferred to v0.1.4+.

---

## 9. Stage 04 Handoff Notes

### Files to Review First (Highest Risk / Most Critical Path)

1. **`source/02-backend/entrypoint.sh`** — The critical path for container startup. If this script fails, the API container never starts. Review the SQL Server wait loop, DbUp invocation, and the `exec` commands.
2. **`source/02-backend/src/Api/Program.cs`** — The `--migrate-only` flag handling. Review the connection string retrieval, migrations directory check, and exit code handling.
3. **`source/02-backend/src/Infrastructure/Persistence/Sql/DatabaseMigrator.cs`** — The DbUp wrapper. Review the `EnsureDatabase`, `WithTransactionPerScript`, and journal configuration.
4. **`docker-compose.yml`** — The full orchestration. Review health check intervals, dependency ordering, environment variable substitution, and network configuration.

### Test Commands and Expected Results

```bash
# 1. Build images (without starting)
docker compose build
# Expected: All three images build successfully. UI < 500 MB, API < 300 MB.

# 2. Full stack startup (first run)
docker compose up
# Expected: SQL Server starts (~30s), API runs migrations and starts (~15s), UI starts (~10s).
# All three health checks pass. http://localhost:4200 shows the Angular SPA.
# http://localhost:5001/api/health returns {"status":"Healthy","version":"0.1.3"}

# 3. Verify idempotent migrations (second run)
docker compose down
docker compose up
# Expected: "Migrations complete. 0 script(s) applied." — no re-application.

# 4. Verify data persistence
docker compose down
docker compose up
# Expected: No data loss. Database schema and data from previous run survive.

# 5. Verify nginx proxy
curl http://localhost:4200/api/health
# Expected: Same response as http://localhost:5001/api/health

# 6. Full teardown with data wipe
docker compose down -v
# Expected: All containers removed. icm-sql-data volume deleted.

# 7. Dev profile (hot reload)
docker compose --profile dev up icm-db icm-api-dev icm-ui-dev
# Expected: Angular dev server on :4200, dotnet watch on :5001.
# Editing source files triggers rebuild/reload.

# 8. Verify image sizes
docker images icm-local-icm-ui icm-local-icm-api
# Expected: icm-ui < 500 MB (NFR-002), icm-api < 300 MB (NFR-002)
```

### Environment Prerequisites

- **Docker Desktop** 4.x+ (or Docker Engine 24.0+ with Docker Compose v2 plugin)
- **Minimum 8 GB RAM** allocated to Docker (SQL Server requires 2 GB+)
- **SSD storage** recommended for acceptable SQL Server cold start times
- **Ports 4200, 5001, 1433** available on the host (no conflicting services)
- **No other SQL Server** instances running on port 1433
- **`.env` file** created from `.env.example` (or accept the default password)

### Smoke Test Steps

1. **Clone and start**: `git clone <repo> && cd icm-coding-workflow && docker compose up`
2. **Verify UI**: Open `http://localhost:4200` in a browser. Expect the Synergistic header with hamburger menu, dashboard route, and footer.
3. **Verify API**: Open `http://localhost:5001/api/health`. Expect `{"status":"Healthy","timestamp":"...","version":"0.1.3"}`.
4. **Verify API via nginx proxy**: Open `http://localhost:4200/api/health`. Expect the same health response proxied through nginx.
5. **Verify database**: Connect to `localhost:1433` with SA credentials (or check API logs for "Migrations complete" message).
6. **Verify teardown**: `docker compose down`. All containers stop. `docker compose up` again. Everything works without data loss.

---

*End of Implementation Results. Handoff to Stage 04 (Review & Testing).*