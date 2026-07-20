# Sequence Diagrams — Code Initialization (v0.1.2)

**Feature**: Code Initialization — project scaffolding, containers, and database setup
**Date**: 2026-07-18
**Version**: v0.1.2

---

## 1. Full-Stack Startup — Happy Path

The developer runs `docker compose up` and the entire stack comes up. This covers FR-001.

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant Docker as docker compose
    participant DbUp as DbUp Engine (API container entrypoint)
    participant SQL as SQL Server (container)
    participant API as .NET API (container, localhost:5001)
    participant Angular as Angular Dev Server (container, localhost:4200)

    Dev->>Docker: docker compose up

    Note over Docker: Check Docker Engine availability

    Note over Docker,SQL: Container 1: SQL Server starts
    Docker->>SQL: Start mcr.microsoft.com/mssql/server
    SQL-->>Docker: Container running, port 1433

    Note over SQL,API: Container 2: API starts (waits for DB healthy)
    Docker->>API: Build & start API container
    API->>DbUp: Run DbUp migrations (entrypoint)
    DbUp->>SQL: SELECT ScriptName FROM dbo.SchemaVersion
    SQL-->>DbUp: [empty — no table yet]
    DbUp->>SQL: CREATE TABLE dbo.SchemaVersion (...)
    SQL-->>DbUp: Command completed
    DbUp->>SQL: INSERT INTO dbo.SchemaVersion (ScriptName) VALUES ('001_CreateSchemaVersion.sql')
    SQL-->>DbUp: 1 row inserted
    DbUp-->>API: Migration complete: 1 script applied
    API->>API: Start Kestrel on http://localhost:5001
    API-->>Docker: Container healthy

    Note over API,Angular: Container 3: Angular starts (waits for API healthy)
    Docker->>Angular: Build & start Angular container
    Angular->>Angular: Vite/esbuild compilation
    Angular->>Angular: Start dev server on http://localhost:4200
    Angular-->>Docker: Container ready

    Docker-->>Dev: ✅ Full stack running!
    Docker-->>Dev: Angular: http://localhost:4200
    Docker-->>Dev: API:     http://localhost:5001
    Docker-->>Dev: DB:      SQL Server (container)
```

---

## 2. Health Check — Happy Path

The Angular app (or browser or Postman) calls the health check endpoint and gets a successful response. Covers FR-007.

```mermaid
sequenceDiagram
    actor User as User / Postman
    participant Browser as Angular SPA (localhost:4200)
    participant Controller as HealthController (localhost:5001)

    Note over User,Controller: Scenario: Verify API is healthy

    User->>Browser: Navigate to app (or open Postman)
    Browser->>Controller: GET /api/health
    Note right of Controller: No auth, no DB call, no dependencies

    Controller->>Controller: GetHealth() action invoked
    Controller->>Controller: new HealthCheckResponse(Status: "Healthy", Timestamp: UTC now, Version: "0.1.2")
    Controller-->>Browser: HTTP 200 OK
    Note right of Controller: Content-Type: application/json

    Browser-->>User: {
    Browser-->>User:   "status": "Healthy",
    Browser-->>User:   "timestamp": "2026-07-19T14:30:00.0000000Z",
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
    participant Controller as HealthController (expected at localhost:5001)

    Note over User,Controller: Scenario: API is stopped — connection refused

    User->>Browser: GET /api/health
    Browser->>Controller: GET http://localhost:5001/api/health
    Controller--xBrowser: Connection refused (ECONNREFUSED)

    alt In Postman
        Browser->>Browser: Test assertion: pm.response.to.have.status(200)
        Browser-->>User: ❌ FAIL: Status code is 200 | Error: connect ECONNREFUSED
    else In Browser / Angular
        Browser->>Browser: HttpClient error handler catches HttpErrorResponse
        Browser-->>User: Console: GET http://localhost:5001/api/health net::ERR_CONNECTION_REFUSED
    end

    Note over User,API: Resolution: Developer runs `docker compose up` to start the API
```

---

## 5. Error Path — Database Migration Failure

DbUp encounters an error while applying a migration script — for example, a syntax error in the SQL or the DB container not running. Covers error handling for FR-008.

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant Docker as docker compose
    participant API as .NET API Container
    participant DbUp as DbUp Engine (API entrypoint)
    participant SQL as SQL Server Container

    Note over Dev,SQL: Scenario: Migration script has invalid SQL

    Dev->>Docker: docker compose up
    Docker->>SQL: Start SQL Server container
    SQL-->>Docker: Container running, port 1433
    Docker->>API: Start API container
    API->>DbUp: Run DbUp migrations (entrypoint)

    DbUp->>SQL: SELECT ScriptName FROM dbo.SchemaVersion
    SQL-->>DbUp: [existing scripts]

    DbUp->>DbUp: Determine pending scripts: 002_BadMigration.sql

    Note over DbUp,SQL: DbUp wraps each script in a transaction

    DbUp->>SQL: BEGIN TRANSACTION
    DbUp->>SQL: -- Contents of 002_BadMigration.sql --
    SQL--xDbUp: ERROR: Incorrect syntax near 'CREAT'

    DbUp->>SQL: ROLLBACK TRANSACTION
    DbUp--xAPI: ❌ Migration failed: 002_BadMigration.sql

    API-->>Docker: Container exits with error
    Docker-->>Dev: ERROR: API container failed to start
    Docker-->>Dev: Logs: Incorrect syntax near 'CREAT'.
    Docker-->>Dev: The database has been rolled back to the last successful migration.

    Note over Dev,SQL: Resolution: Fix syntax error, re-run `docker compose up` (idempotent — no manual cleanup needed)
```

---

## 6. Prerequisites Check — Missing Dependency

The developer runs `docker compose up` without having Docker Engine installed. Docker Desktop provides helpful guidance.

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant Docker as Docker CLI

    Note over Dev,Docker: Scenario: Docker Engine is not installed or not running

    Dev->>Docker: docker compose up

    Docker--xDev: 'docker' is not recognized as a cmdlet

    Note over Dev,Docker: Resolution: Install Docker Desktop, then re-run `docker compose up`
```

---

## 7. Full-Stack Startup — Already Initialized (Idempotent Re-run)

The developer runs `docker compose up` again after the database already exists. DbUp detects no pending migrations and skips the database phase. Covers the idempotency requirement in FR-008 and ADR-004.

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant Docker as docker compose
    participant API as .NET API Container
    participant DbUp as DbUp Engine (API entrypoint)
    participant SQL as SQL Server Container
    participant Angular as Angular Dev Server Container

    Dev->>Docker: docker compose up (second run)

    Docker->>SQL: Start SQL Server container (already initialized)
    SQL-->>Docker: Container running

    Docker->>API: Start API container
    API->>DbUp: Run DbUp migrations (entrypoint)
    DbUp->>SQL: SELECT ScriptName FROM dbo.SchemaVersion
    SQL-->>DbUp: ['001_CreateSchemaVersion.sql']
    DbUp->>DbUp: Compare with migrations/ folder
    DbUp->>DbUp: All scripts already applied — nothing to run
    DbUp-->>API: Migration complete: 0 scripts applied (up to date)
    API->>API: Start Kestrel on http://localhost:5001
    API-->>Docker: Container healthy

    Docker->>Angular: Start Angular container
    Angular->>Angular: Compile & serve
    Angular-->>Docker: Container ready

    Docker-->>Dev: ✅ Full stack running!
    Docker-->>Dev: Angular: http://localhost:4200
    Docker-->>Dev: API:     http://localhost:5001
```

---

## Diagram Summary

| Diagram | Type | Covers | Key Principles |
|---|---|---|---|
| 1. Full-Stack Startup | Happy path | FR-001, FR-007, FR-008 | Separation of Concerns, Idempotency |
| 2. Health Check | Happy path | FR-007 | Single Responsibility, Controller Pattern |
| 3. Navigation Menu Toggle | Happy path | FR-002, FR-003 | Event-driven UI, Component isolation |
| 4. API Unavailable | Error path | FR-007 | Graceful degradation, clear error messaging |
| 5. Database Migration Failure | Error path | FR-008 | Transactional safety, auto-rollback |
| 6. Missing Prerequisites | Error path | FR-001 | Fail fast, helpful guidance |
| 7. Idempotent Re-run | Happy path | FR-008, ADR-004 | Idempotency, repeatable infrastructure |