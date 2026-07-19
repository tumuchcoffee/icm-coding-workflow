# Sequence Diagrams — Code Initialization (v0.1.2)

**Feature**: Code Initialization — project scaffolding, containers, and database setup
**Date**: 2026-07-18
**Version**: v0.1.2

---

## 1. Full-Stack Startup — Happy Path

The developer runs `run.ps1` and the entire stack comes up. This covers FR-001.

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant Script as run.ps1
    participant DbUp as DbUp Engine
    participant SQL as SQL Server LocalDB
    participant API as .NET API (localhost:5001)
    participant Angular as Angular Dev Server (localhost:4200)

    Dev->>Script: ./run.ps1

    Note over Script: Check prerequisites
    Script->>Script: Verify dotnet --version
    Script->>Script: Verify node --version
    Script->>Script: Verify sqllocaldb info

    Note over Script,SQL: Phase 1: Database Migrations
    Script->>DbUp: Execute migrations
    DbUp->>SQL: SELECT ScriptName FROM dbo.SchemaVersion
    SQL-->>DbUp: [empty — no table yet]
    DbUp->>SQL: CREATE TABLE dbo.SchemaVersion (...)
    SQL-->>DbUp: Command completed
    DbUp->>SQL: INSERT INTO dbo.SchemaVersion (ScriptName) VALUES ('001_CreateSchemaVersion.sql')
    SQL-->>DbUp: 1 row inserted
    DbUp-->>Script: Migration complete: 1 script applied

    Note over Script,API: Phase 2: Start .NET API
    Script->>API: dotnet run (background)
    API->>API: Build configuration
    API->>API: Map endpoints (Health)
    API->>API: Start Kestrel on http://localhost:5001
    API-->>Script: Now listening on http://localhost:5001

    Note over Script,Angular: Phase 3: Start Angular Dev Server
    Script->>Angular: npm start (foreground)
    Angular->>Angular: Vite/esbuild compilation
    Angular->>Angular: Start dev server on http://localhost:4200
    Angular-->>Script: Compiled successfully

    Script-->>Dev: ✅ Full stack running!
    Script-->>Dev: Angular: http://localhost:4200
    Script-->>Dev: API:     http://localhost:5001
    Script-->>Dev: DB:      (LocalDB)\MSSQLLocalDB\Synergistic
```

---

## 2. Health Check — Happy Path

The Angular app (or browser or Postman) calls the health check endpoint and gets a successful response. Covers FR-007.

```mermaid
sequenceDiagram
    actor User as User / Postman
    participant Browser as Angular SPA (localhost:4200)
    participant API as .NET API (localhost:5001)

    Note over User,API: Scenario: Verify API is healthy

    User->>Browser: Navigate to app (or open Postman)
    Browser->>API: GET /api/health
    Note right of API: No auth, no DB call, no dependencies

    API->>API: MapGet("/api/health") handler invoked
    API->>API: new HealthCheckResponse(Status: "Healthy", Timestamp: UTC now, Version: "0.1.2")
    API-->>Browser: HTTP 200 OK
    Note right of API: Content-Type: application/json

    Browser-->>User: {
    Browser-->>User:   "status": "Healthy",
    Browser-->>User:   "timestamp": "2026-07-18T14:30:00.0000000Z",
    Browser-->>User:   "version": "0.1.2"
    Browser-->>User: }
```

---

## 3. Navigation Menu Toggle — Happy Path

The user clicks the hamburger icon to open the slide-out menu, then closes it. Covers FR-002 and FR-003.

```mermaid
sequenceDiagram
    actor User
    participant Header as HeaderComponent
    participant Shell as AppShellComponent
    participant Menu as MenuComponent (p-sidebar)

    Note over User,Menu: Open the Menu

    User->>Header: Click hamburger icon (p-button)
    Header->>Shell: menuToggle.emit()
    Shell->>Shell: this.menuOpen.set(true)
    Shell->>Menu: [isOpen] = true (signal binding)
    Menu->>Menu: p-sidebar visible = true
    Menu->>Menu: Animate slide-in from left (256px)
    Menu-->>User: Menu panel visible with nav links

    Note over User,Menu: Close the Menu (click outside or close icon)

    User->>Menu: Click outside panel (or close icon)
    Menu->>Menu: p-sidebar visible = false
    Menu->>Menu: Animate slide-out
    Menu->>Shell: closed.emit()
    Shell->>Shell: this.menuOpen.set(false)
    Menu-->>User: Menu panel hidden
```

---

## 4. Error Path — API Unavailable

The user tries to access the health check while the .NET API is not running. The request fails with a connection error. Covers error handling for FR-007.

```mermaid
sequenceDiagram
    actor User
    participant Browser as Angular SPA / Postman
    participant API as .NET API (expected at localhost:5001)

    Note over User,API: Scenario: API is stopped — connection refused

    User->>Browser: GET /api/health
    Browser->>API: GET http://localhost:5001/api/health
    API--xBrowser: Connection refused (ECONNREFUSED)

    alt In Postman
        Browser->>Browser: Test assertion: pm.response.to.have.status(200)
        Browser-->>User: ❌ FAIL: Status code is 200 | Error: connect ECONNREFUSED
    else In Browser / Angular
        Browser->>Browser: HttpClient error handler catches HttpErrorResponse
        Browser-->>User: Console: GET http://localhost:5001/api/health net::ERR_CONNECTION_REFUSED
    end

    Note over User,API: Resolution: Developer runs run.ps1 to start the API
```

---

## 5. Error Path — Database Migration Failure

DbUp encounters an error while applying a migration script — for example, a syntax error in the SQL or LocalDB not running. Covers error handling for FR-008.

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant Script as run.ps1
    participant DbUp as DbUp Engine
    participant SQL as SQL Server LocalDB

    Note over Dev,SQL: Scenario: Migration script has invalid SQL

    Dev->>Script: ./run.ps1
    Script->>Script: Prerequisites pass
    Script->>DbUp: Execute migrations

    DbUp->>SQL: SELECT ScriptName FROM dbo.SchemaVersion
    SQL-->>DbUp: [existing scripts]

    DbUp->>DbUp: Determine pending scripts: 002_BadMigration.sql

    Note over DbUp,SQL: DbUp wraps each script in a transaction

    DbUp->>SQL: BEGIN TRANSACTION
    DbUp->>SQL: -- Contents of 002_BadMigration.sql --
    SQL--xDbUp: ERROR: Incorrect syntax near 'CREAT'

    DbUp->>SQL: ROLLBACK TRANSACTION
    DbUp--xScript: ❌ Migration failed: 002_BadMigration.sql

    Script->>Script: Catch DbUp exception
    Script-->>Dev: ERROR: Database migration failed.
    Script-->>Dev: Script: 002_BadMigration.sql
    Script-->>Dev: Details: Incorrect syntax near 'CREAT'.
    Script-->>Dev: The database has been rolled back to the last successful migration.

    Note over Dev,SQL: Resolution: Fix syntax error, re-run run.ps1 (idempotent — no manual cleanup needed)
```

---

## 6. Prerequisites Check — Missing Dependency

The developer runs `run.ps1` without having the .NET 10 SDK installed. The script detects this early and provides helpful guidance.

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant Script as run.ps1

    Note over Dev,Script: Scenario: .NET 10 SDK is not installed

    Dev->>Script: ./run.ps1

    Script->>Script: Check: dotnet --version
    Script--xScript: 'dotnet' is not recognized

    Script->>Script: Check: node --version
    Script->>Script: v22.5.1 ✅

    Script->>Script: Check: sqllocaldb info
    Script->>Script: MSSQLLocalDB ✅

    Script-->>Dev: ❌ Missing prerequisite: .NET 10 SDK
    Script-->>Dev: Download: https://dotnet.microsoft.com/en-us/download/dotnet/10.0
    Script-->>Dev: After installing, re-run: ./run.ps1

    Note over Dev,Script: Resolution: Install .NET 10 SDK, re-run
```

---

## 7. Full-Stack Startup — Already Initialized (Idempotent Re-run)

The developer runs `run.ps1` again after the database already exists. DbUp detects no pending migrations and skips the database phase. Covers the idempotency requirement in FR-008 and ADR-004.

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant Script as run.ps1
    participant DbUp as DbUp Engine
    participant SQL as SQL Server LocalDB
    participant API as .NET API
    participant Angular as Angular Dev Server

    Dev->>Script: ./run.ps1 (second run)

    Note over Script: Prerequisites: all pass ✅

    Script->>DbUp: Execute migrations
    DbUp->>SQL: SELECT ScriptName FROM dbo.SchemaVersion
    SQL-->>DbUp: ['001_CreateSchemaVersion.sql']
    DbUp->>DbUp: Compare with migrations/ folder
    DbUp->>DbUp: All scripts already applied — nothing to run
    DbUp-->>Script: Migration complete: 0 scripts applied (up to date)

    Script->>API: dotnet run
    API-->>Script: Now listening on http://localhost:5001

    Script->>Angular: npm start
    Angular-->>Script: Compiled successfully

    Script-->>Dev: ✅ Full stack running!
    Script-->>Dev: Angular: http://localhost:4200
    Script-->>Dev: API:     http://localhost:5001
```

---

## Diagram Summary

| Diagram | Type | Covers | Key Principles |
|---|---|---|---|
| 1. Full-Stack Startup | Happy path | FR-001, FR-007, FR-008 | Separation of Concerns, Idempotency |
| 2. Health Check | Happy path | FR-007 | Single Responsibility, Minimal API |
| 3. Navigation Menu Toggle | Happy path | FR-002, FR-003 | Event-driven UI, Component isolation |
| 4. API Unavailable | Error path | FR-007 | Graceful degradation, clear error messaging |
| 5. Database Migration Failure | Error path | FR-008 | Transactional safety, auto-rollback |
| 6. Missing Prerequisites | Error path | FR-001 | Fail fast, helpful guidance |
| 7. Idempotent Re-run | Happy path | FR-008, ADR-004 | Idempotency, repeatable infrastructure |