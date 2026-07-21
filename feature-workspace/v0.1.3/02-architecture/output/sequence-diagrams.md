# Sequence Diagrams — Docker Local Development (v0.1.3)

**Feature**: Docker Local Development — Dockerfiles, Compose orchestration, and local containerized workflow
**Date**: 2026-07-20
**Version**: v0.1.3

---

## 1. Full-Stack Cold Start — Happy Path

The developer clones the repository and runs `docker compose up` for the first time. Docker builds images, starts all containers, and the full stack becomes healthy. Covers FR-001 through FR-007.

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant DC as Docker Compose
    participant Docker as Docker Engine
    participant SQL as icm-db (SQL Server)
    participant API as icm-api (.NET API)
    participant UI as icm-ui (Angular + nginx)

    Dev->>DC: docker compose up (first run)

    Note over DC,Docker: ── Phase 1: Image Build ──

    DC->>Docker: Build icm-ui image (source/01-ui/Dockerfile)
    Docker->>Docker: Stage 1: npm ci + ng build (Node 22 Alpine)
    Docker->>Docker: Stage 2: Copy dist/ to nginx:alpine
    Docker-->>DC: icm-ui image built ✅

    DC->>Docker: Build icm-api image (source/02-backend/Dockerfile)
    Docker->>Docker: Stage 1: dotnet restore + publish (.NET SDK 10)
    Docker->>Docker: Stage 2: Copy publish/ to aspnet:10.0
    Docker-->>DC: icm-api image built ✅

    DC->>Docker: Pull mcr.microsoft.com/mssql/server:2022-latest
    Docker-->>DC: SQL Server image cached/pulled

    Note over DC,Docker: ── Phase 2: Network & Volume ──

    DC->>Docker: Create network icm-network (bridge)
    DC->>Docker: Create volume icm-sql-data

    Note over DC,SQL: ── Phase 3: Start SQL Server ──

    DC->>Docker: Start icm-db container
    Docker->>SQL: Container init: SQL Server engine
    SQL->>SQL: Initialize master database
    Note right of SQL: Health check polls every 10s
    SQL->>SQL: /opt/mssql-tools18/bin/sqlcmd -Q "SELECT 1"
    SQL-->>Docker: healthy ✅

    Note over DC,API: ── Phase 4: Start .NET API ──

    DC->>Docker: Start icm-api container (depends_on: icm-db healthy)
    Docker->>API: Run entrypoint.sh

    API->>SQL: Wait for TCP connectivity (poll loop)
    loop Until ready (max 60s)
        API->>SQL: sqlcmd -S icm-db -U sa -Q "SELECT 1"
        SQL-->>API: 1
    end

    API->>API: dotnet Api.dll --migrate-only
    API->>SQL: SELECT ScriptName FROM dbo.SchemaVersion
    SQL-->>API: 001_CreateSchemaVersion.sql

    Note over API,SQL: No pending migrations (all applied)

    API->>API: exec dotnet Api.dll
    API->>API: Kestrel listening on http://+:5001
    API->>API: /api/health → 200 OK
    API-->>Docker: healthy ✅

    Note over DC,UI: ── Phase 5: Start Angular UI ──

    DC->>Docker: Start icm-ui container (depends_on: icm-api healthy)
    Docker->>UI: Start nginx
    UI->>UI: nginx listening on :80
    UI->>UI: / → index.html (SPA shell)
    UI-->>Docker: healthy ✅

    DC-->>Dev: ✅ Full stack running!
    DC-->>Dev: Angular UI:  http://localhost:4200
    DC-->>Dev: .NET API:   http://localhost:5001
    DC-->>Dev: SQL Server: localhost:1433 (optional)
```

**Performance Note:** NFR-001 requires cold start within 120 seconds. The bottleneck is SQL Server initialization (~30–45 seconds cold, ~10 seconds warm with persistent volume). API and UI build times are amortized by Docker layer caching — first build may take 2–3 minutes, but subsequent `docker compose up` runs use cached layers and complete within the 120-second threshold.

---

## 2. Health Check Request — Happy Path

The developer verifies the API is running by calling the health endpoint from the browser or Postman. Covers FR-002 (API is functional).

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant Browser as Browser / Postman (host)
    participant UI as icm-ui nginx (localhost:4200)
    participant API as icm-api (internal:5001)
    participant DB as icm-db (internal:1433)

    Note over Dev,DB: Scenario: Verify the API is healthy

    Dev->>Browser: GET http://localhost:5001/api/health

    alt Direct API call (bypasses nginx)
        Browser->>API: GET /api/health
    else Through nginx proxy
        Dev->>Browser: Navigate to http://localhost:4200
        Browser->>UI: GET /
        UI-->>Browser: index.html (Angular SPA)
        Note over Browser,UI: Angular app calls /api/health
        Browser->>UI: GET /api/health
        UI->>API: proxy_pass http://icm-api:5001
    end

    API->>API: HealthController.GetHealth()
    Note right of API: No DB dependency in v0.1.2 health check
    API->>API: new HealthCheckResponse { Status = "Healthy", ... }
    API-->>Browser: HTTP 200 OK

    Note over Browser: {
    Note over Browser:   "status": "Healthy",
    Note over Browser:   "timestamp": "2026-07-20T14:30:00.0000000Z",
    Note over Browser:   "version": "0.1.2"
    Note over Browser: }
```

---

## 3. Stack Teardown — Happy Path

The developer runs `docker compose down` to stop and clean up all containers. Covers FR-004 (teardown is as simple as startup).

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant DC as Docker Compose
    participant Docker as Docker Engine
    participant UI as icm-ui
    participant API as icm-api
    participant SQL as icm-db
    participant Vol as icm-sql-data (volume)

    Dev->>DC: docker compose down

    Note over DC,Docker: ── Stop & Remove Containers ──

    DC->>Docker: Stop icm-ui container
    Docker->>UI: SIGTERM → nginx graceful shutdown
    Docker->>Docker: Remove icm-ui container

    DC->>Docker: Stop icm-api container
    Docker->>API: SIGTERM → Kestrel graceful shutdown
    Docker->>Docker: Remove icm-api container

    DC->>Docker: Stop icm-db container
    Docker->>SQL: SIGTERM → SQL Server checkpoint + shutdown
    Docker->>Docker: Remove icm-db container

    Note over DC,Vol: ── Preserve Data Volume ──
    Note over Vol: icm-sql-data volume is NOT removed
    Note over Vol: Database files persist for next `docker compose up`

    DC-->>Dev: ✅ All containers stopped and removed
    DC-->>Dev: Data volume preserved (icm-sql-data)
```

---

## 4. Database Migration — Happy Path (New Migration)

A new migration script (`002_AddWidgetsTable.sql`) is added. On the next `docker compose up`, DbUp detects the pending script and applies it. Covers FR-007 (idempotent migrations).

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant DC as Docker Compose
    participant API as icm-api
    participant DbUp as DbUp Engine
    participant SQL as icm-db

    Note over Dev,SQL: Scenario: A new migration script has been added

    Dev->>Dev: Add 002_AddWidgetsTable.sql to source/03-sql/migrations/
    Dev->>DC: docker compose up

    DC->>API: Start icm-api container
    API->>API: Run entrypoint.sh

    API->>SQL: Wait for SQL Server (poll)
    SQL-->>API: Ready

    API->>DbUp: dotnet Api.dll --migrate-only
    DbUp->>SQL: SELECT ScriptName FROM dbo.SchemaVersion ORDER BY ScriptName
    SQL-->>DbUp: [001_CreateSchemaVersion.sql]

    DbUp->>DbUp: Pending scripts: [002_AddWidgetsTable.sql]

    DbUp->>SQL: BEGIN TRANSACTION
    DbUp->>SQL: -- Contents of 002_AddWidgetsTable.sql --
    DbUp->>SQL: CREATE TABLE dbo.Widgets (...)
    SQL-->>DbUp: Command completed successfully

    DbUp->>SQL: INSERT INTO dbo.SchemaVersion (ScriptName) VALUES ('002_AddWidgetsTable.sql')
    SQL-->>DbUp: 1 row inserted

    DbUp->>SQL: COMMIT TRANSACTION
    DbUp-->>API: Migration complete: 1 script applied

    API->>API: exec dotnet Api.dll (start Kestrel)
    API-->>DC: healthy ✅

    DC-->>Dev: ✅ Stack running with updated schema
```

---

## 5. Error Path — SQL Server Not Ready (Timeout)

The API container starts but SQL Server does not become ready within 60 seconds. The entrypoint script times out and the API container exits with an error. Covers error handling for FR-007.

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant DC as Docker Compose
    participant API as icm-api
    participant SQL as icm-db

    Note over Dev,SQL: Scenario: SQL Server is slow to start or unreachable

    Dev->>DC: docker compose up
    DC->>SQL: Start icm-db

    Note over SQL: Container starts but takes > 60s to be ready
    Note over SQL: (e.g., volume recovery on slow disk, resource contention)

    DC->>API: Start icm-api (depends_on: icm-db healthy... but it's not)
    API->>API: Run entrypoint.sh

    loop Poll SQL Server (every 5s, max 12 retries)
        API->>SQL: sqlcmd -S icm-db ... -Q "SELECT 1"
        SQL--xAPI: Connection timeout / SQL Server not ready
        API->>API: Retry in 5 seconds...
    end

    Note over API: Retries exhausted after 60 seconds

    API->>API: echo "ERROR: SQL Server did not become ready"
    API->>API: exit 1
    API--xDC: Container exited with code 1

    Note over DC: Compose restart policy: unless-stopped
    DC->>API: Restart icm-api (attempt 2)

    Note over SQL: SQL Server finally finishes initializing
    SQL-->>SQL: healthy ✅

    Note over DC: depends_on condition now satisfied

    API->>API: Run entrypoint.sh (attempt 2)
    API->>SQL: sqlcmd -S icm-db ... -Q "SELECT 1"
    SQL-->>API: 1
    API->>API: Migrations + API start
    API-->>DC: healthy ✅

    Note over Dev: Resolution: Automatic retry via restart policy
```

---

## 6. Error Path — Migration Script Failure

A migration script contains a SQL syntax error. DbUp rolls back the transaction and the API container fails to start. Covers error handling for FR-007.

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant DC as Docker Compose
    participant API as icm-api
    participant DbUp as DbUp Engine
    participant SQL as icm-db

    Note over Dev,SQL: Scenario: New migration script has invalid SQL

    Dev->>Dev: Add 002_BadMigration.sql (contains syntax error)
    Dev->>DC: docker compose up

    DC->>API: Start icm-api
    API->>API: Run entrypoint.sh
    API->>SQL: Wait for SQL Server
    SQL-->>API: Ready

    API->>DbUp: dotnet Api.dll --migrate-only
    DbUp->>SQL: SELECT ScriptName FROM dbo.SchemaVersion
    SQL-->>DbUp: [001_CreateSchemaVersion.sql]

    DbUp->>DbUp: Pending scripts: [002_BadMigration.sql]

    DbUp->>SQL: BEGIN TRANSACTION
    DbUp->>SQL: -- 002_BadMigration.sql --
    SQL--xDbUp: ERROR: Incorrect syntax near 'CREAT'
    DbUp->>SQL: ROLLBACK TRANSACTION

    DbUp--xAPI: ❌ Migration failed: 002_BadMigration.sql
    Note over API: entrypoint.sh detects non-zero exit code

    API->>API: echo "ERROR: Database migration failed."
    API->>API: exit 1
    API--xDC: Container exited with code 1

    Note over DC: Compose restart policy: unless-stopped
    DC->>API: Restart icm-api (attempt 2)
    API->>API: entrypoint.sh → DbUp runs again
    DbUp->>SQL: SELECT ScriptName FROM dbo.SchemaVersion
    SQL-->>DbUp: [001_CreateSchemaVersion.sql]
    Note over DbUp: 002_BadMigration.sql still pending (was rolled back)

    DbUp->>SQL: BEGIN TRANSACTION
    SQL--xDbUp: Same error
    DbUp->>SQL: ROLLBACK TRANSACTION
    API--xDC: exit 1 (again)

    Note over DC: Container keeps restarting and failing
    DC-->>Dev: 🔴 icm-api in restart loop

    Note over Dev: Resolution: Fix migration script, then `docker compose down && docker compose up`
```

---

## 7. Error Path — API Unreachable from nginx Proxy

The API container is stopped or crashes after the UI container has started. nginx returns 502 Bad Gateway when proxying API requests. Covers error handling for FR-006 (external access).

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant Browser as Browser (localhost:4200)
    participant UI as icm-ui nginx
    participant API as icm-api

    Note over Dev,API: Scenario: API crashes after initial healthy state

    Browser->>UI: GET /
    UI-->>Browser: index.html (Angular SPA loads fine)

    Note over Browser: User navigates to a page that calls /api/...

    Browser->>UI: GET /api/tenants
    UI->>API: proxy_pass http://icm-api:5001/api/tenants
    API--xUI: Connection refused (container stopped)

    UI->>UI: nginx error: upstream connection failed
    UI-->>Browser: HTTP 502 Bad Gateway

    Note over Browser: Angular error interceptor catches HttpErrorResponse
    Note over Browser: User sees error toast / error page

    Note over Dev: Resolution: Check API logs, restart icm-api container
    Note over Dev: docker compose restart icm-api
```

---

## 8. Angular UI Navigation via nginx Proxy — Happy Path

The developer accesses the Angular SPA through the nginx container, which serves static files and proxies API requests. Covers FR-001 and FR-006.

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant Browser as Browser
    participant UI as icm-ui nginx (:80)
    participant API as icm-api (:5001)
    participant DB as icm-db (:1433)

    Dev->>Browser: Navigate to http://localhost:4200

    Note over Browser,UI: ── Initial Page Load ──
    Browser->>UI: GET /
    UI->>UI: Serve /usr/share/nginx/html/index.html
    UI-->>Browser: index.html + CSS + JS bundles

    Note over Browser: Angular bootstrap

    Note over Browser,UI: ── API Request via nginx Proxy ──
    Browser->>UI: GET /api/health
    UI->>UI: location /api/ → proxy_pass http://icm-api:5001
    UI->>API: GET /api/health
    API-->>UI: 200 OK { "status": "Healthy", ... }
    UI-->>Browser: 200 OK

    Note over Browser: ── SPA Routing ──
    Browser->>Browser: Router navigates to /dashboard
    Browser->>UI: GET /dashboard (Angular route)
    UI->>UI: try_files $uri $uri/ /index.html
    Note over UI: /dashboard is not a file → serve index.html
    UI-->>Browser: index.html (Angular handles /dashboard route)

    Note over Browser,UI: ── Angular Lazy-Loaded Feature ──
    Browser->>UI: GET /dashboard-chunk.js
    UI->>UI: Serve static file from /usr/share/nginx/html/
    UI-->>Browser: dashboard-chunk.js
```

---

## 9. Development Mode — Hot Reload Flow

The developer uses the `dev` profile to get hot reload. They edit a TypeScript file and see the change reflected immediately. Covers FR-008.

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant Editor as VS Code (host)
    participant UI as icm-ui-dev (Angular dev server)
    participant API as icm-api-dev (dotnet watch)
    participant DB as icm-db

    Dev->>Dev: docker compose --profile dev up

    Note over Editor,DB: All three dev containers running with volume mounts

    Note over Editor,UI: ── Angular Hot Reload ──

    Dev->>Editor: Edit app.component.ts
    Editor->>UI: File change detected (volume mount, --poll 2000)
    UI->>UI: Angular dev server: recompile changed module
    UI->>UI: HMR (Hot Module Replacement) — update browser
    UI-->>Dev: Browser auto-refreshes with new code

    Note over Editor,API: ── .NET Hot Reload ──

    Dev->>Editor: Edit TenantsController.cs
    Editor->>API: File change detected (volume mount)
    API->>API: dotnet watch: detect file change
    API->>API: dotnet watch: restart process with new code
    API-->>Dev: API restarted with changes

    Note over Editor,DB: ── Database persists across reloads ──
    Note over DB: SQL Server data volume unchanged
    Note over DB: No migration re-run needed (idempotent)
```

---

## 10. Cross-Platform Startup — Windows / macOS / Linux

The same `docker compose up` works identically on all three platforms. Covers NFR-004.

```mermaid
sequenceDiagram
    actor Win as Windows Developer
    actor Mac as macOS Developer
    actor Linux as Linux Developer

    Note over Win,Linux: All three run: git clone + docker compose up

    par Windows (Docker Desktop)
        Win->>Win: docker compose up
        Win->>Win: Docker Desktop WSL2 backend
        Win->>Win: Containers run in WSL2 VM
        Win->>Win: Ports mapped to Windows host
        Win->>Win: localhost:4200 → UI
        Win->>Win: localhost:5001 → API
    and macOS (Docker Desktop)
        Mac->>Mac: docker compose up
        Mac->>Mac: Docker Desktop LinuxKit VM
        Mac->>Mac: Containers run in VM
        Mac->>Mac: Rosetta emulation for SQL Server (arm64)
        Mac->>Mac: localhost:4200 → UI
        Mac->>Mac: localhost:5001 → API
    and Linux (Docker Engine)
        Linux->>Linux: docker compose up
        Linux->>Linux: Native Docker Engine
        Linux->>Linux: Containers run natively
        Linux->>Linux: localhost:4200 → UI
        Linux->>Linux: localhost:5001 → API
    end

    Note over Win,Linux: ✅ Identical developer experience on all platforms
```

---

*All diagrams trace to functional requirements (FR-001 through FR-008) and non-functional requirements (NFR-001, NFR-002, NFR-003, NFR-004) from Stage 01.*
