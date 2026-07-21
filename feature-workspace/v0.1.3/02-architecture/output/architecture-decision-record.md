# Architecture Decision Records — Docker Local Development (v0.1.3)

**Feature**: Docker Local Development — Dockerfiles, Compose orchestration, and local containerized workflow
**Date**: 2026-07-20
**Version**: v0.1.3

---

## ADR-010: Docker Compose over Manual Container Orchestration

**Status**: Accepted
**Date**: 2026-07-20
**Context**:
FR-004 requires a single command to start the entire application stack. The system architecture (`docs/system-architecture.md`, Section 8.2) specifies Docker images from multi-stage builds as part of the CI/CD pipeline. The requirements mandate three services — Angular UI, .NET API, and SQL Server — that must communicate over a shared network (FR-005) with external access for the UI and API (FR-006).

**Decision**:
Use **Docker Compose** (the `docker compose` plugin, not the legacy `docker-compose` Python tool) to define and orchestrate all three containers. The `docker-compose.yml` file lives at the repository root and declares:

1. **`icm-db`** — SQL Server 2022 Developer Edition (`mcr.microsoft.com/mssql/server:2022-latest`), with a named volume for data persistence and a health check that verifies the SQL Server process is accepting connections.
2. **`icm-api`** — .NET 10 Web API, built from `source/02-backend/Dockerfile`, depends on `icm-db` being healthy, runs DbUp migrations on startup, and listens on port 5001.
3. **`icm-ui`** — Angular 19 SPA, built from `source/01-ui/Dockerfile`, depends on `icm-api` being healthy, and listens on port 4200.

All three containers attach to a single user-defined bridge network (`icm-network`). The UI and API expose ports to the host; the database port (1433) is exposed only on the internal network per NFR-003.

**Alternatives Considered**:
1. **Manual `docker run` commands with shell scripts** — Rejected: contradicts FR-004 ("single command"). Manual orchestration is error-prone (ordering, networking, cleanup) and not portable across OSes.
2. **Docker Swarm / Kubernetes for local dev** — Rejected: massive overkill. The requirements are for a single-developer local workflow, not multi-node orchestration. Compose is the standard tool for local multi-container development.
3. **`docker-compose` (legacy Python v1)** — Rejected: `docker compose` (v2 plugin, Go) is the current standard and is bundled with Docker Desktop. No reason to use the legacy tool.
4. **Separate Compose files for dev vs. production** — Partially deferred. The v0.1.3 scope is local development only. A production Compose file (or Kubernetes manifests) is out of scope per the backlog.

**Consequences**:
- **Easier**: A single `docker compose up` starts the entire stack. `docker compose down` tears it down cleanly. Docker Compose handles dependency ordering via `depends_on` with health checks, network creation, and volume management. Developers do not need to install Node.js, .NET SDK, or SQL Server — only Docker Desktop.
- **Harder**: The Compose file must be maintained as services evolve. Startup time is bounded by the slowest container (SQL Server cold start). Volume mounts for hot reload (FR-008) add complexity to the Compose configuration.
- **Follow-up**: If the project grows beyond three services (e.g., Redis, Azure Functions emulator, Service Bus emulator), the Compose file will need profiles or separate override files. This is out of scope for v0.1.3.
- **Risk**: Low. Docker Compose is the industry standard for local multi-container development. The pattern is well-established.

**Principles Applied**:
- **Convention over Configuration** — Docker Compose is the expected tool for "run my multi-container app locally." Using it follows established developer expectations.
- **Separation of Concerns** — Each service's Dockerfile defines how to build it; the Compose file defines how they run together. These are distinct concerns.
- **Simple over Complex** — Compose is the simplest tool that satisfies all requirements. No orchestration framework, no custom scripts, no platform-specific code.

---

## ADR-011: Multi-stage Docker Builds for All Custom Images

**Status**: Accepted
**Date**: 2026-07-20
**Context**:
NFR-002 sets image size thresholds: < 500 MB for the UI image and < 300 MB for the API image. Both the Angular SPA and the .NET API require SDK toolchains to build (Node.js/npm and .NET SDK respectively) but only need lightweight runtimes to serve. Installing build toolchains in the final image would bloat image sizes and increase attack surface.

**Decision**:
Use **multi-stage Docker builds** for both the UI and API Dockerfiles. Each Dockerfile has at least two stages:

1. **Build stage** — Uses the full SDK image (Node.js with npm for Angular; .NET SDK for the API). Compiles the application, runs tests if configured, and produces production-ready artifacts.
2. **Runtime stage** — Uses a minimal runtime image (nginx:alpine for Angular; `mcr.microsoft.com/dotnet/aspnet:10.0` for the API). Copies only the build artifacts from the build stage. No SDK, no source code, no build tools.

For the Angular UI, an optional **development stage** (using `docker compose` profiles or a separate `docker-compose.override.yml`) provides the Angular dev server with volume mounts for hot reload (FR-008). This stage is not used in the default `docker compose up` flow.

**Alternatives Considered**:
1. **Single-stage build with full SDK in runtime** — Rejected: violates NFR-002. A Node.js + Angular CLI image is > 1 GB. A .NET SDK image is > 800 MB. Neither is acceptable for a developer tool that may be pulled frequently.
2. **Pre-built artifacts (no Docker build)** — Rejected: contradicts FR-001 and FR-002 ("Dockerfile that builds and serves"). The Dockerfile must be self-contained and produce a working image from source.
3. **Alpine-based SDK images** — Considered but rejected for the .NET API. The .NET SDK Alpine image exists but is not the default; using it would introduce an untested variant. The standard `mcr.microsoft.com/dotnet/sdk:10.0` and `mcr.microsoft.com/dotnet/aspnet:10.0` images are the supported path.

**Consequences**:
- **Easier**: Final images are small (UI: nginx serving static files, ~50–100 MB; API: trimmed self-contained runtime, ~150–250 MB). Faster pulls, less disk usage, smaller attack surface. The build stage is cached by Docker layer caching — unchanged dependencies are not re-downloaded.
- **Harder**: Dockerfiles are more complex (two stages instead of one). Developers must understand the distinction between build and runtime images. The build stage's layer cache must be managed carefully to avoid cache invalidation on every source change.
- **Follow-up**: The development stage (hot reload) must be designed in the component design phase. Docker Compose profiles or override files will be needed to switch between development and production-style builds.
- **Risk**: Low. Multi-stage builds are a Docker best practice and are well-supported by all Docker tooling.

**Principles Applied**:
- **Separation of Concerns** — Build and runtime are distinct phases with distinct dependencies. Multi-stage builds enforce this separation at the Dockerfile level.
- **Least Privilege** — The runtime image contains only what is needed to serve the application. No compilers, no SDKs, no package managers.
- **Cost-Aware** — Smaller images reduce storage costs, network transfer time, and CI/CD pipeline duration.

---

## ADR-012: nginx for Angular SPA Serving over Angular Dev Server in Production-style Builds

**Status**: Accepted
**Date**: 2026-07-20
**Context**:
FR-001 requires a Dockerfile that builds and serves the Angular SPA. The Angular dev server (`ng serve`) is designed for development with hot reload, not for serving production builds. For the default `docker compose up` flow, the container should serve the production-built SPA — fast, lightweight, and representative of what will be deployed. The development flow (hot reload, volume mounts) is covered separately by FR-008.

**Decision**:
Use **nginx:alpine** as the runtime base image for the Angular UI Dockerfile's production stage. The build stage runs `ng build --configuration production` to produce static files in `dist/`. The runtime stage copies those files to nginx's default serving directory (`/usr/share/nginx/html/`) and adds a custom `nginx.conf` that:

- Serves the Angular SPA on port 80 (mapped to host port 4200 via Compose)
- Configures Angular's HTML5 pushState routing (`try_files $uri $uri/ /index.html`)
- Adds a reverse proxy for `/api/` requests to `http://icm-api:5001` (internal Docker network)
- Sets appropriate cache headers for static assets (JavaScript, CSS, images: long cache; index.html: no-cache)

**Alternatives Considered**:
1. **Angular dev server (`ng serve`) as the default** — Rejected: the dev server is not designed for production-style serving. It is slower to start, consumes more memory, and does not represent the deployed application. Hot reload belongs in the development stage (FR-008), not the default flow.
2. **Node.js HTTP server (e.g., `http-server` or `serve`)** — Rejected: nginx is purpose-built for serving static files with high performance, low memory, and built-in reverse proxy capabilities. Adding Node.js to the runtime image defeats the purpose of multi-stage builds.
3. **Caddy** — Considered: simpler configuration than nginx. Rejected: nginx is more widely known, more documented, and the team already has nginx expertise. The system architecture uses Azure Front Door for CDN, which is nginx-compatible.
4. **Serve API proxy through Angular CLI proxy config** — Rejected: the Angular CLI proxy only works with `ng serve` (dev server). It is not available in production builds.

**Consequences**:
- **Easier**: The UI container serves the same static files that would be deployed to Azure Static Web Apps / CDN in production. The nginx reverse proxy eliminates the need for CORS configuration in development — API requests from the browser go to the same origin (the UI container), which proxies to the API internally. This mirrors the production pattern where Front Door routes `/api/` to the App Service.
- **Harder**: The nginx configuration must be maintained alongside the Angular app. Developers must understand that the UI container proxies API requests — this is not the same as the Angular dev server proxy. CORS issues that appear in the dev server (FR-008) but not in the nginx proxy (or vice versa) must be debugged.
- **Follow-up**: If the application grows to need server-side rendering (SSR), the nginx approach will need to be replaced with Angular Universal or a Node.js server. This is not in scope for the current SPA architecture.
- **Risk**: Low. nginx reverse proxy for SPAs is a well-established pattern.

**Principles Applied**:
- **Separation of Concerns** — The UI container's job is to serve static files and proxy API requests. nginx is the right tool for this job. Angular's job is to produce the static files.
- **Consistency** — The Docker Compose flow produces a deployment that is as close as possible to the production architecture (Static Web Apps + App Service).
- **Simple over Complex** — nginx configuration for an SPA is a single `server` block with a `try_files` directive and a `proxy_pass`. It is simpler than maintaining a Node.js server process.

---

## ADR-013: DbUp Migration Execution at API Container Entrypoint

**Status**: Accepted
**Date**: 2026-07-20
**Context**:
FR-007 requires database migrations to be applied automatically on startup. The v0.1.2 architecture established DbUp as the migration tool (ADR-004) with the `001_CreateSchemaVersion.sql` migration already in place. In the Docker Compose flow, the API container must apply any pending migrations before the API starts serving requests. The SQL Server container may not be immediately ready when the API container starts — it needs time to initialize.

**Decision**:
The API container's **entrypoint** (a shell script or inline `ENTRYPOINT`/`CMD` instruction) will:

1. **Wait for SQL Server** — Poll the database port (1433) or execute a lightweight `SELECT 1` query until the SQL Server container is ready. Use a retry loop with a timeout (max 60 seconds) to avoid indefinite waiting.
2. **Run DbUp** — Execute the DbUp migration engine, which scans `source/03-sql/migrations/` (embedded as resources or copied into the container), queries `dbo.SchemaVersion` for applied scripts, and runs any pending scripts in order.
3. **Start the API** — Once migrations complete (or are confirmed already applied), start the .NET Kestrel server (`dotnet Synergistic.Api.dll`).

The entrypoint is defined in the API Dockerfile and overrides the default .NET runtime image entrypoint. The health check for the API container verifies that the API is responding to HTTP requests (not just that the process is running) — this ensures that Compose does not mark the API as healthy until migrations are complete and the server is listening.

**Alternatives Considered**:
1. **Run DbUp in `Program.cs` before `app.Run()`** — Rejected: this couples migration execution to the API process. If migrations fail, the API process crashes. The entrypoint approach keeps migration logic separate from the API code and allows the container to retry without restarting the API process.
2. **Separate migration container (init container pattern)** — Rejected: adds complexity for a local dev workflow. A separate container that runs migrations and exits would require Docker Compose's `depends_on` with `condition: service_completed_successfully`, which is only available in Compose v3+. The entrypoint approach is simpler and sufficient for local development.
3. **Manual migration execution (`docker exec`)** — Rejected: contradicts FR-007 ("automatically on startup"). Manual steps are error-prone and not idempotent by default.
4. **DbUp in `Program.cs` with Polly retry for SQL connectivity** — Considered as a v0.1.3 refinement. The `Program.cs` approach could work with Polly retries for the initial connection, but the entrypoint script provides clearer separation of concerns (wait for DB → migrate → start API) and is easier to debug (logs are separate from API logs).

**Consequences**:
- **Easier**: The database is always at the correct schema version when the API starts. No manual migration steps. Idempotent — running `docker compose up` multiple times does not re-apply existing migrations. The entrypoint script provides clear, separate log output for the wait/migrate/start phases.
- **Harder**: The API Dockerfile is more complex (custom entrypoint script). The entrypoint script must be maintained as a shell script (Linux) in the repository. Debugging migration failures requires reading the entrypoint script output, not just the API logs.
- **Follow-up**: In production, migrations may be run as a separate CI/CD step or an init container. The entrypoint approach is a local development convenience. The migration execution strategy for production should be defined when the CI/CD pipeline is implemented.
- **Risk**: Low. DbUp is explicitly designed for this pattern. The risk of a migration failure blocking the API is intentional — we want the API to fail fast if the database is at the wrong schema version.

**Principles Applied**:
- **Separation of Concerns** — The entrypoint script handles startup orchestration (wait, migrate, start). The API code handles business logic. These are distinct concerns.
- **Idempotency** — DbUp guarantees idempotent migrations. The wait-and-retry loop guarantees the API does not start until the database is ready.
- **Fail Fast** — If migrations fail, the container fails to start. This is the correct behavior — a running API with an incorrect schema is worse than a stopped API.

---

## ADR-014: Single Docker Network with Internal DNS for Service Discovery

**Status**: Accepted
**Date**: 2026-07-20
**Context**:
FR-005 requires all containers to communicate over a single Docker network. The API must reach the database, and the UI must proxy to the API. Docker Compose creates a default network, but explicit network configuration provides better control and documentation.

**Decision**:
Use a **single user-defined bridge network** named `icm-network` in the Docker Compose file. All three services attach to this network. Docker's built-in DNS resolution allows containers to address each other by service name:

- `icm-db` — SQL Server, reachable from the API at `Server=icm-db,1433`
- `icm-api` — .NET API, reachable from the UI at `http://icm-api:5001`
- `icm-ui` — Angular SPA (nginx), reachable from the host at `http://localhost:4200`

The user-defined bridge network provides automatic DNS resolution between containers (unlike the default bridge network, which requires `--link` flags). It also isolates the application stack from other Docker networks on the developer's machine.

**Alternatives Considered**:
1. **Default Compose network (no explicit network)** — Rejected: while Compose creates a default network automatically, explicit naming makes the network identifiable in `docker network ls` and allows future services to reference it by name. The default network is adequate for simple cases but implicit behavior is harder to debug.
2. **Host networking (`network_mode: host`)** — Rejected: host networking bypasses Docker's network isolation and only works on Linux. It violates NFR-004 (cross-platform compatibility). Port conflicts with other services on the host are likely.
3. **Multiple networks (e.g., separate frontend/backend networks)** — Rejected: overengineered for a three-container local dev setup. A single network is simpler and sufficient. The production architecture uses VNet integration and private endpoints, which are not relevant to local development.

**Consequences**:
- **Easier**: Service discovery works out of the box via Docker DNS. Connection strings are simple and predictable. The network is isolated and named.
- **Harder**: None significant. This is the standard Docker Compose pattern.
- **Risk**: Very low. User-defined bridge networks are a core Docker feature.

**Principles Applied**:
- **Separation of Concerns** — The network is a dedicated infrastructure concern, defined declaratively in the Compose file.
- **Convention over Configuration** — Docker Compose's service-name-as-hostname convention is used instead of custom hostname mappings.
- **Simple over Complex** — One network for three containers. No overlay networks, no network policies, no custom subnets.

---

## ADR-015: Development vs. Production Build Modes via Compose Profiles

**Status**: Accepted
**Date**: 2026-07-20
**Context**:
FR-008 requires source code volume mounts for hot reload, but the default `docker compose up` flow should produce a production-style build. These two modes have conflicting requirements: the production mode builds static files and serves them with nginx; the development mode runs the Angular dev server with volume mounts and the .NET API with `dotnet watch`. A single Compose file cannot express both modes cleanly without conditional logic.

**Decision**:
Use **Docker Compose profiles** to separate the two modes:

- **Default profile** (no `--profile` flag): Production-style builds. The UI runs nginx serving pre-built static files. The API runs the published .NET assembly. No volume mounts. No hot reload. This is the mode used by `docker compose up`.
- **`dev` profile** (`docker compose up --profile dev`): Development mode. The UI runs the Angular dev server (`ng serve`) with the `src/` directory mounted as a volume. The API runs `dotnet watch` with the `src/` directory mounted. Both support hot reload.

The Dockerfiles use multi-stage builds with a `development` target that is used only when the `dev` profile is active. The default (production) target is used otherwise.

**Alternatives Considered**:
1. **Separate `docker-compose.override.yml`** — Considered: this is the traditional Docker Compose pattern for dev overrides. Rejected in favor of profiles because profiles are a first-class Compose feature (since v1.28) and do not require a separate file. However, the override file approach is still viable and can be used as an alternative.
2. **Always development mode (hot reload only)** — Rejected: the default flow should represent the production deployment as closely as possible. Hot reload is a developer convenience, not the default behavior.
3. **Always production mode (no hot reload)** — Rejected: contradicts FR-008. Volume mounts and hot reload are explicitly required for developer productivity.
4. **Environment variables to switch modes** — Considered: environment variables can conditionally set commands and volumes, but Compose does not support conditional `volumes:` or `build:` sections based on environment variables. Profiles are the cleanest way to express conditional service configurations.

**Consequences**:
- **Easier**: Two clearly separated workflows. `docker compose up` for a quick sanity check of the production build. `docker compose up --profile dev` for active development with hot reload. No manual file editing to switch modes.
- **Harder**: The Compose file is more complex (duplicate service definitions for dev mode or extensive use of YAML anchors). The Dockerfiles must support both build targets. Developers must know which profile to use.
- **Follow-up**: The `dev` profile implementation in Stage 03 will define the specific volume mounts, port mappings, and commands for hot reload. The Dockerfiles must include `development` stage targets.
- **Risk**: Low. Compose profiles are a stable, well-documented feature.

**Principles Applied**:
- **Separation of Concerns** — Development and production are distinct use cases with distinct configurations. Profiles keep them separate.
- **Convention over Configuration** — The default (no flag) behavior is production-style. The `--profile dev` flag is the explicit opt-in for development mode.
- **Open/Closed** — The default profile is closed for modification (production behavior is fixed). The dev profile is open for extension (additional dev tools can be added).

---

## ADR-016: SQL Server 2022 Developer Edition Container

**Status**: Accepted
**Date**: 2026-07-20
**Context**:
FR-003 requires a Docker Compose service definition for SQL Server. GAP-002 identified that no SQL Server edition or version was specified. The system architecture targets Azure SQL Database for production, but local development needs a SQL Server instance that is compatible with Azure SQL (T-SQL surface area, feature parity) and runs on any developer machine.

**Decision**:
Use the **official Microsoft SQL Server 2022 Developer Edition** image (`mcr.microsoft.com/mssql/server:2022-latest`). This is:

- **Free** — Developer Edition is licensed for non-production use
- **Compatible with Azure SQL** — SQL Server 2022 has the highest T-SQL feature parity with Azure SQL Database
- **Cross-platform** — Runs on Windows, macOS (Apple Silicon via Rosetta), and Linux (amd64)
- **Actively maintained** — Official Microsoft image with regular security updates

The container is configured with:
- `ACCEPT_EULA=Y` — Required to accept the license agreement
- `MSSQL_SA_PASSWORD` — A strong password from a Compose environment variable (default: `SynergisticDev123!` for local dev only)
- `MSSQL_PID=Developer` — Explicitly selects Developer Edition
- Named volume (`icm-sql-data`) mounted at `/var/opt/mssql/data` for data persistence
- Health check using `sqlcmd` or a TCP probe on port 1433

**Alternatives Considered**:
1. **SQL Server Express** — Rejected: Express Edition has database size limits (10 GB) and reduced feature set (no SQL Agent, limited replication). Developer Edition has no limits and is also free for development.
2. **Azure SQL Edge** — Rejected: Azure SQL Edge is a lightweight container for IoT/edge scenarios. It has a reduced T-SQL surface area and lacks features like MARS, certain spatial types, and CLR integration. It is not a good representation of the production Azure SQL Database.
3. **SQL Server LocalDB** — Rejected: LocalDB does not run in a container. It is a Windows-only feature. The Docker approach requires a containerized SQL Server instance.
4. **PostgreSQL or MySQL** — Rejected: the system architecture specifies SQL Server. Changing the database engine for local development would create drift between environments.

**Consequences**:
- **Easier**: Full SQL Server feature set. No database size limits. Compatible with all T-SQL features used in production. The named volume preserves data across `docker compose down` and container restarts.
- **Harder**: The SQL Server image is large (~1.5 GB compressed). Initial pull may take several minutes on slow connections. The container requires at least 2 GB of RAM. Apple Silicon Macs run the container via Rosetta emulation, which is slower than native ARM.
- **Follow-up**: If the team adopts ARM-based development machines widely, an ARM-native SQL Server container may become necessary. Microsoft has announced ARM support for SQL Server 2025; this ADR should be revisited when that becomes available.
- **Risk**: Medium. The image size and resource requirements may deter developers with limited bandwidth or RAM. NFR-001 (120-second cold start) is achievable with the SQL Server container on SSD storage.

**Principles Applied**:
- **Consistency** — The development database should be as close to the production database as possible. SQL Server Developer Edition is the closest available containerized option.
- **Cost-Aware** — Developer Edition is free. No Azure SQL Database costs for local development.
- **Least Privilege** — The SA password is scoped to the local Docker network. No Azure AD authentication, no managed identity — these are production concerns.

---

## ADR-017: .NET 10 Runtime Image with Self-Contained Deployment

**Status**: Accepted
**Date**: 2026-07-20
**Context**:
NFR-002 requires the API image to be < 300 MB. The .NET 10 runtime image (`mcr.microsoft.com/dotnet/aspnet:10.0`) is approximately 220 MB uncompressed. Adding the application binaries should keep the total under 300 MB. The API must run on Linux containers (the App Service in production uses Linux).

**Decision**:
Use the **.NET 10 ASP.NET runtime image** (`mcr.microsoft.com/dotnet/aspnet:10.0`) as the runtime base for the API Dockerfile. The API is published as a **framework-dependent deployment** (FDD) — not self-contained — because the runtime image already includes the .NET 10 runtime. This keeps the application binaries small and the image size within the NFR-002 threshold.

The build stage uses `mcr.microsoft.com/dotnet/sdk:10.0` to restore, build, and publish:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Api/Api.csproj", "Api/"]
COPY ["src/Application/Application.csproj", "Application/"]
COPY ["src/Domain/Domain.csproj", "Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "Api/Api.csproj"
COPY . .
RUN dotnet publish "Api/Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./entrypoint.sh"]
```

The `AS runtime` stage uses the official ASP.NET image which includes the runtime, ASP.NET Core framework, and the Linux environment. The `ENTRYPOINT` is a shell script that waits for SQL Server, runs DbUp, and starts the API.

**Alternatives Considered**:
1. **Self-contained deployment (SCD)** — Rejected: a self-contained .NET 10 deployment for Linux is ~70–90 MB of framework binaries plus the application. While it eliminates the need for the runtime image, the total image size would be larger (OS base + ~80 MB framework + ~20 MB app). The FDD approach is more efficient because the runtime is shared in the base image layer.
2. **Alpine runtime image** — Considered: `mcr.microsoft.com/dotnet/aspnet:10.0-alpine` is smaller (~110 MB) but uses musl libc instead of glibc. Some .NET features (System.Drawing, certain globalization features) behave differently on Alpine. The production App Service runs Debian-based Linux; matching that environment reduces surprises.
3. **Chiseled / distroless images** — Considered: Microsoft's Ubuntu Chiseled images are ultra-minimal (~50 MB for ASP.NET). Rejected for v0.1.3 because they lack a shell, making the entrypoint script (wait for DB, run DbUp) impossible. A chiseled image could be used in production where migrations run separately.
4. **Windows containers** — Rejected: the system architecture specifies Linux App Service Plans. Windows containers are larger, slower to start, and not cross-platform.

**Consequences**:
- **Easier**: The FDD approach produces the smallest possible image while maintaining compatibility with the production environment. The published output is ~15–20 MB, on top of the ~220 MB base image, staying well under the 300 MB threshold.
- **Harder**: The entrypoint script requires a shell in the runtime image. The Debian-based ASP.NET image includes bash, so this is not an issue. If we move to chiseled images in the future, the entrypoint must be rethought.
- **Risk**: Low. The official Microsoft .NET images are the standard for containerized .NET applications.

**Principles Applied**:
- **Consistency** — The runtime environment matches the production App Service (Debian Linux, .NET 10).
- **Cost-Aware** — FDD minimizes image size, reducing pull time and storage costs.
- **Simple over Complex** — The standard Microsoft images are the simplest path. No custom base images, no chiseled layers, no multi-arch manifests.

---

## Summary of ADRs

| ADR | Title | Status |
|-----|-------|--------|
| ADR-010 | Docker Compose over Manual Container Orchestration | Accepted |
| ADR-011 | Multi-stage Docker Builds for All Custom Images | Accepted |
| ADR-012 | nginx for Angular SPA Serving over Angular Dev Server | Accepted |
| ADR-013 | DbUp Migration Execution at API Container Entrypoint | Accepted |
| ADR-014 | Single Docker Network with Internal DNS for Service Discovery | Accepted |
| ADR-015 | Development vs. Production Build Modes via Compose Profiles | Accepted |
| ADR-016 | SQL Server 2022 Developer Edition Container | Accepted |
| ADR-017 | .NET 10 Runtime Image with Framework-Dependent Deployment | Accepted |

---

*All ADRs trace to requirements from Stage 01. See the traceability matrix in `backlog-and-prioritization.md` for requirement-to-story mapping.*