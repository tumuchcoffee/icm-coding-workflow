# Feature Requirements — Docker Local Development

> **Tier:** Lightweight (Score: 6) — single stakeholder, well-understood domain, low risk, no regulatory exposure.
> **Sources:** `references/requirements.md`
> **Generated:** 2026-07-20

---

## User Personas

### User Persona: Software Engineer
**Role:** A developer contributing to the Synergistic ICM Admin Panel codebase.
**Goal:** Run and verify the full application stack locally using Docker containers without manual toolchain setup.

---

## Functional Requirements

### Dockerfile per Service

#### FR-001: Dockerfile for Angular UI
**As a** software engineer,
**I want** a Dockerfile that builds and serves the Angular SPA,
**so that** I can run the UI in a container without installing Node.js or Angular CLI locally.

**Acceptance Criteria:**
- [ ] GIVEN I have the Dockerfile in the `source/01-ui/` directory
      WHEN I run `docker build -t icm-ui .`
      THEN a container image is produced with the Angular app served on port 4200
- [ ] GIVEN the UI container is running
      WHEN I navigate to `http://localhost:4200`
      THEN the Angular SPA renders successfully

**Sources:** requirements.md

---

#### FR-002: Dockerfile for .NET API
**As a** software engineer,
**I want** a Dockerfile that builds and runs the .NET 10 Web API,
**so that** I can run the backend in a container without installing the .NET SDK locally.

**Acceptance Criteria:**
- [ ] GIVEN I have the Dockerfile in the `source/02-backend/` directory
      WHEN I run `docker build -t icm-api .`
      THEN a container image is produced with the .NET API listening on port 5001
- [ ] GIVEN the API container is running
      WHEN I send a GET request to `http://localhost:5001`
      THEN the API responds with a non-error HTTP status

**Sources:** requirements.md

---

#### FR-003: Dockerfile for SQL Server
**As a** software engineer,
**I want** a Docker Compose service definition for SQL Server,
**so that** I can run a local database without installing SQL Server natively.

**Acceptance Criteria:**
- [ ] GIVEN the Docker Compose stack is running
      WHEN I check the SQL Server container status
      THEN the container is healthy and accepting connections on port 1433
- [ ] GIVEN the SQL Server container is running
      WHEN the API container starts
      THEN the API can connect to the database using the connection string from the Compose environment

**Sources:** requirements.md

---

### Orchestration

#### FR-004: Docker Compose File
**As a** software engineer,
**I want** a single `docker-compose.yml` file at the repository root,
**so that** I can start the entire application stack with one command.

**Acceptance Criteria:**
- [ ] GIVEN I have cloned the repository
      WHEN I run `docker compose up`
      THEN all three containers (UI, API, SQL Server) start and reach a healthy state
- [ ] GIVEN the stack is running
      WHEN I run `docker compose down`
      THEN all containers are stopped and removed

**Sources:** requirements.md

---

#### FR-005: Shared Docker Network
**As a** software engineer,
**I want** all containers to communicate over a single Docker network,
**so that** the API can reach the database and the UI can proxy to the API without exposing internal ports to the host unnecessarily.

**Acceptance Criteria:**
- [ ] GIVEN the Docker Compose stack is running
      WHEN I inspect the Docker network
      THEN all three containers are attached to the same user-defined bridge network
- [ ] GIVEN all containers are on the shared network
      WHEN the API resolves the database hostname
      THEN it resolves to the SQL Server container via Docker DNS

**Sources:** requirements.md

---

#### FR-006: External Access from Host
**As a** software engineer,
**I want** the UI and API to be accessible from my host machine's browser and tools,
**so that** I can interact with the application as if it were deployed.

**Acceptance Criteria:**
- [ ] GIVEN the Docker Compose stack is running
      WHEN I navigate to `http://localhost:4200` on my host machine
      THEN the Angular SPA loads
- [ ] GIVEN the Docker Compose stack is running
      WHEN I send a request to `http://localhost:5001` from my host machine
      THEN the API responds

**Sources:** requirements.md

---

### Database Initialization

#### FR-007: Database Migrations on Startup
**As a** software engineer,
**I want** the API container to apply DbUp migrations automatically on startup,
**so that** the database schema is always up to date without manual intervention.

**Acceptance Criteria:**
- [ ] GIVEN a fresh SQL Server container with no database
      WHEN the API container starts for the first time
      THEN the database is created and all migrations from `source/03-sql/migrations/` are applied
- [ ] GIVEN the SQL Server container is restarted but the database volume persists
      WHEN the API container starts again
      THEN only new migrations are applied (idempotent)

**Sources:** requirements.md (implicit — inferred from `001_CreateSchemaVersion.sql` and DbUp pattern in Infrastructure layer)

---

### Developer Experience

#### FR-008: Source Code Volume Mounts
**As a** software engineer,
**I want** my local source code to be mounted into the containers,
**so that** I can edit code on my host and see changes reflected without rebuilding images.

**Acceptance Criteria:**
- [ ] GIVEN the Docker Compose stack is running with volume mounts
      WHEN I edit a TypeScript file in `source/01-ui/src/`
      THEN the Angular dev server detects the change and hot-reloads the browser
- [ ] GIVEN the Docker Compose stack is running with volume mounts
      WHEN I edit a C# file in `source/02-backend/src/`
      THEN the .NET API restarts or applies the change via `dotnet watch`

**Sources:** requirements.md (implicit — developer workflow inferred from "run or confirm their changes")

---

## Gaps & Contradictions

| ID | Type | Description |
|----|------|-------------|
| GAP-001 | Missing detail | The source does not specify whether hot-reload (volume mounts) is required or if a production-style build-and-serve pattern is sufficient. FR-008 assumes the former for developer productivity. |
| GAP-002 | Missing detail | No SQL Server edition or version is specified. Assumed SQL Server 2022 Developer Edition. |
| GAP-003 | Missing detail | No specification for where the `docker-compose.yml` should live. Assumed repository root. |

---

*No contradictions found between sources — only one source file was provided.*