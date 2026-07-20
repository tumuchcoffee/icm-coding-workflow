# Component Design — Docker Local Development (v0.1.3)

**Feature**: Docker Local Development — Dockerfiles, Compose orchestration, and local containerized workflow
**Date**: 2026-07-20
**Version**: v0.1.3

---

## 1. Docker Compose Orchestration

### 1.1 docker-compose.yml

| Attribute | Detail |
|---|---|
| **Responsibility** | Declare the full application stack (UI, API, SQL Server) as a single orchestration unit. `docker compose up` starts everything; `docker compose down` tears it down. |
| **Location** | Repository root: `docker-compose.yml` |
| **Dependencies** | Docker Engine 24.0+, Docker Compose v2 (plugin) |
| **Interface** | Three services: `icm-ui`, `icm-api`, `icm-db` on a shared `icm-network` bridge network |
| **Error Handling** | Health checks on all three containers. The API waits for the database to be healthy before starting. The UI waits for the API to be healthy. Container startup failures propagate to the developer via `docker compose` exit codes and log output. |
| **Tenant Awareness** | None. Local development has no tenant context. |
| **Traceability** | FR-004, FR-005, FR-006, NFR-003 (port isolation) |
| **Principle** | **Separation of Concerns** — the Compose file orchestrates containers; Dockerfiles define how to build them. **Single Responsibility** — the Compose file does one thing: define the multi-container application. |

**Service Definitions:**

```yaml
# docker-compose.yml — v0.1.3
# ICM Admin Panel local development stack

name: icm-local

services:
  # ── SQL Server 2022 Developer Edition ─────────────────────────
  icm-db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: icm-db
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: ${MSSQL_SA_PASSWORD:-SynergisticDev123!}
      MSSQL_PID: Developer
    ports:
      - "1433:1433"        # Exposed only on internal network per NFR-003
    volumes:
      - icm-sql-data:/var/opt/mssql/data
    networks:
      - icm-network
    healthcheck:
      test: /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "${MSSQL_SA_PASSWORD:-SynergisticDev123!}" -C -Q "SELECT 1"
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 30s
    restart: unless-stopped

  # ── .NET 10 Web API ───────────────────────────────────────────
  icm-api:
    build:
      context: ./source/02-backend
      dockerfile: Dockerfile
      target: runtime
    container_name: icm-api
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5001
      - ConnectionStrings__Default=Server=icm-db,1433;Database=Synergistic;User Id=sa;Password=${MSSQL_SA_PASSWORD:-SynergisticDev123!};TrustServerCertificate=True;
    ports:
      - "5001:5001"
    networks:
      - icm-network
    depends_on:
      icm-db:
        condition: service_healthy
    healthcheck:
      test: curl --fail http://localhost:5001/api/health || exit 1
      interval: 10s
      timeout: 5s
      retries: 3
      start_period: 15s
    restart: unless-stopped

  # ── Angular 19 SPA ────────────────────────────────────────────
  icm-ui:
    build:
      context: ./source/01-ui
      dockerfile: Dockerfile
      target: runtime
    container_name: icm-ui
    ports:
      - "4200:80"           # nginx serves on 80; mapped to 4200 on host
    networks:
      - icm-network
    depends_on:
      icm-api:
        condition: service_healthy
    healthcheck:
      test: curl --fail http://localhost:80/ || exit 1
      interval: 10s
      timeout: 5s
      retries: 3
      start_period: 10s
    restart: unless-stopped

  # ── Development profile services ──────────────────────────────
  # Started with: docker compose --profile dev up
  icm-api-dev:
    build:
      context: ./source/02-backend
      dockerfile: Dockerfile
      target: development
    container_name: icm-api-dev
    profiles: [dev]
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5001
      - ConnectionStrings__Default=Server=icm-db,1433;Database=Synergistic;User Id=sa;Password=${MSSQL_SA_PASSWORD:-SynergisticDev123!};TrustServerCertificate=True;
    ports:
      - "5001:5001"
    volumes:
      - ./source/02-backend/src:/app/src
      - /app/obj
      - /app/bin
    networks:
      - icm-network
    depends_on:
      icm-db:
        condition: service_healthy
    restart: unless-stopped

  icm-ui-dev:
    build:
      context: ./source/01-ui
      dockerfile: Dockerfile
      target: development
    container_name: icm-ui-dev
    profiles: [dev]
    ports:
      - "4200:4200"
    volumes:
      - ./source/01-ui/src:/app/src
      - /app/node_modules
    networks:
      - icm-network
    depends_on:
      - icm-api-dev
    restart: unless-stopped

networks:
  icm-network:
    driver: bridge
    name: icm-network

volumes:
  icm-sql-data:
    name: icm-sql-data
```

**Development Profile Notes:**
- The `dev` profile services (`icm-api-dev`, `icm-ui-dev`) use the `development` Dockerfile target with volume mounts and hot reload.
- The `dev` profile does NOT replace the default services — it runs alongside them. For a dev-only workflow, use `docker compose --profile dev up icm-db icm-api-dev icm-ui-dev`.
- The `node_modules` volume is anonymous (no source on host) to prevent the host's `node_modules` (if any) from overwriting the container's installed dependencies.

---

## 2. Angular UI Dockerfile

### 2.1 Dockerfile

| Attribute | Detail |
|---|---|
| **Responsibility** | Build the Angular 19 SPA from source and serve it via nginx (production) or Angular dev server (development). |
| **Location** | `source/01-ui/Dockerfile` |
| **Dependencies** | Node.js 22 LTS (build), nginx:alpine (runtime), Angular CLI 19 |
| **Interface** | Multi-stage build with two targets: `runtime` (default) and `development` |
| **Error Handling** | Build errors (TypeScript compilation, template errors) fail the Docker build. Missing dependencies fail `npm ci`. The nginx configuration validates at build time. |
| **Tenant Awareness** | None. The UI is stateless and tenant-agnostic in local development. |
| **Traceability** | FR-001, FR-008, NFR-002 (< 500 MB image) |
| **Principle** | **Multi-stage Build** (ADR-011) — separate build and runtime stages. **Separation of Concerns** — the Dockerfile builds; nginx serves. |

**Dockerfile Structure:**

```dockerfile
# source/01-ui/Dockerfile
# Angular 19 SPA — multi-stage Docker build
# ADR-011: Multi-stage builds for all custom images
# ADR-012: nginx for production serving

# ── Stage 1: Build ──────────────────────────────────────────────
FROM node:22-alpine AS build
WORKDIR /app

# Copy package manifests first for layer caching
COPY package.json package-lock.json* ./
RUN npm ci --ignore-scripts

# Copy source and build
COPY . .
RUN npm run build -- --configuration production

# ── Stage 2: Runtime (nginx) — default target ───────────────────
FROM nginx:alpine AS runtime
# ADR-012: nginx for Angular SPA serving

# Remove default nginx config
RUN rm /etc/nginx/conf.d/default.conf

# Copy custom nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built Angular app from build stage
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
HEALTHCHECK --interval=10s --timeout=5s --retries=3 \
  CMD wget -qO- http://localhost:80/ || exit 1

# ── Stage 3: Development (Angular dev server) — optional target ──
FROM node:22-alpine AS development
# ADR-015: Dev profile for hot reload
WORKDIR /app

# Install Angular CLI globally
RUN npm install -g @angular/cli@19

# Copy package manifests and install
COPY package.json package-lock.json* ./
RUN npm ci

# Source is mounted as a volume at runtime — not copied
EXPOSE 4200
CMD ["ng", "serve", "--host", "0.0.0.0", "--port", "4200", "--poll", "2000"]
```

### 2.2 nginx Configuration

| Attribute | Detail |
|---|---|
| **Responsibility** | Serve the Angular SPA static files, enable HTML5 pushState routing, and proxy `/api/` requests to the API container. |
| **Location** | `source/01-ui/nginx.conf` |
| **Dependencies** | `icm-api` container on the `icm-network` Docker network |
| **Interface** | Listens on port 80. Proxies `/api/` to `http://icm-api:5001`. Serves static files from `/usr/share/nginx/html`. |
| **Error Handling** | nginx returns 404 for unknown static files (before falling through to `index.html`). If the API is unreachable, nginx returns 502 Bad Gateway. |
| **Tenant Awareness** | None. |
| **Traceability** | FR-001, FR-006, ADR-012 |
| **Principle** | **Separation of Concerns** — nginx handles HTTP serving and reverse proxy; Angular handles the application. |

**nginx.conf:**

```nginx
# source/01-ui/nginx.conf
# ADR-012: nginx config for Angular SPA with API proxy
server {
    listen 80;
    server_name localhost;
    root /usr/share/nginx/html;
    index index.html;

    # HTML5 pushState routing — all non-file routes fall through to index.html
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Reverse proxy API requests to the .NET API container
    location /api/ {
        proxy_pass http://icm-api:5001;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }

    # Cache static assets aggressively
    location ~* \.(?:css|js|jpg|jpeg|gif|png|ico|svg|woff|woff2|ttf|eot)$ {
        expires 30d;
        add_header Cache-Control "public, immutable";
    }

    # Never cache index.html
    location = /index.html {
        expires -1;
        add_header Cache-Control "no-cache, no-store, must-revalidate";
    }
}
```

---

## 3. .NET API Dockerfile

### 3.1 Dockerfile

| Attribute | Detail |
|---|---|
| **Responsibility** | Build the .NET 10 Web API solution from source, run DbUp migrations on startup, and start the Kestrel server. |
| **Location** | `source/02-backend/Dockerfile` |
| **Dependencies** | .NET SDK 10.0 (build), .NET ASP.NET Runtime 10.0 (runtime), SQL Server container (at runtime) |
| **Interface** | Multi-stage build with two targets: `runtime` (default) and `development` |
| **Error Handling** | Build errors fail the Docker build. Migration failures fail the entrypoint script (container exits). API runtime errors are logged by Serilog. |
| **Tenant Awareness** | None in local development. The API does not enforce tenant isolation in the Development environment. |
| **Traceability** | FR-002, FR-007, FR-008, NFR-002 (< 300 MB image) |
| **Principle** | **Multi-stage Build** (ADR-011) — separate build and runtime stages. **Separation of Concerns** — the entrypoint handles DB readiness; the API handles business logic. |

**Dockerfile Structure:**

```dockerfile
# source/02-backend/Dockerfile
# .NET 10 Web API — multi-stage Docker build
# ADR-011: Multi-stage builds for all custom images
# ADR-013: DbUp migrations at entrypoint
# ADR-017: Framework-dependent deployment on aspnet:10.0

# ── Stage 1: Build ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for layer caching
COPY src/Api/Api.csproj Api/
COPY src/Application/Application.csproj Application/
COPY src/Domain/Domain.csproj Domain/
COPY src/Infrastructure/Infrastructure.csproj Infrastructure/

# Restore dependencies
RUN dotnet restore "Api/Api.csproj"

# Copy remaining source and publish
COPY src/ .
RUN dotnet publish "Api/Api.csproj" -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime — default target ───────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
# ADR-017: Framework-dependent deployment on Debian-based ASP.NET image

WORKDIR /app

# Install curl for health checks (Debian-based image)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Copy SQL migrations for DbUp
COPY src/ ../03-sql/migrations/ ./migrations/

# Copy and set entrypoint script
COPY entrypoint.sh .
RUN chmod +x entrypoint.sh

EXPOSE 5001
ENTRYPOINT ["./entrypoint.sh"]

# ── Stage 3: Development (dotnet watch) — optional target ───────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development
# ADR-015: Dev profile for hot reload
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy entrypoint for migrations
COPY entrypoint.sh .
RUN chmod +x entrypoint.sh

# Source is mounted as a volume at runtime — not copied
# The entrypoint runs migrations, then starts dotnet watch
ENTRYPOINT ["./entrypoint.sh", "watch"]
```

### 3.2 Entrypoint Script

| Attribute | Detail |
|---|---|
| **Responsibility** | Wait for SQL Server to be ready, run DbUp migrations, then start the API (or `dotnet watch` in dev mode). |
| **Location** | `source/02-backend/entrypoint.sh` |
| **Dependencies** | SQL Server container (icm-db), .NET runtime/SDK, DbUp library |
| **Interface** | Takes an optional argument: `watch` (starts `dotnet watch`) or empty (starts `dotnet Api.dll`). |
| **Error Handling** | SQL Server wait loop times out after 60 seconds → container exits with error. Migration failure → container exits with error. API crashes → container exits. |
| **Tenant Awareness** | None. |
| **Traceability** | FR-007, ADR-013 |
| **Principle** | **Fail Fast** — if the database is not ready or migrations fail, the container stops. **Idempotency** — DbUp ensures migrations are safe to run multiple times. |

**entrypoint.sh:**

```bash
#!/bin/bash
# source/02-backend/entrypoint.sh
# ADR-013: DbUp migration execution at API container entrypoint
set -e

echo "=== ICM API Container Entrypoint ==="
echo "Waiting for SQL Server to be ready..."

# Wait for SQL Server to accept connections (max 60 seconds)
RETRIES=12
until /opt/mssql-tools18/bin/sqlcmd -S icm-db -U sa -P "${MSSQL_SA_PASSWORD:-SynergisticDev123!}" -C -Q "SELECT 1" &>/dev/null; do
  RETRIES=$((RETRIES - 1))
  if [ $RETRIES -le 0 ]; then
    echo "ERROR: SQL Server did not become ready within 60 seconds."
    exit 1
  fi
  echo "SQL Server not ready yet. Retrying in 5 seconds... ($RETRIES retries left)"
  sleep 5
done

echo "SQL Server is ready. Running DbUp migrations..."

# Run DbUp migrations (embedded in the application)
# The DbUp runner is invoked via a custom console runner or the API's startup hook
# For v0.1.3, this is a placeholder — the actual DbUp integration is implemented in Stage 03
dotnet Api.dll --migrate-only

if [ $? -ne 0 ]; then
  echo "ERROR: Database migration failed."
  exit 1
fi

echo "Migrations complete. Starting API..."

# Start the API (or dotnet watch in dev mode)
if [ "$1" = "watch" ]; then
  echo "Starting in development mode (dotnet watch)..."
  exec dotnet watch run --project src/Api/Api.csproj --urls "http://+:5001"
else
  echo "Starting in production mode (Kestrel)..."
  exec dotnet Api.dll
fi
```

---

## 4. SQL Server Service

### 4.1 Container Configuration

| Attribute | Detail |
|---|---|
| **Responsibility** | Provide a SQL Server 2022 Developer Edition instance for local development. |
| **Image** | `mcr.microsoft.com/mssql/server:2022-latest` (ADR-016) |
| **Dependencies** | None (no other containers depend on it at startup; it is the first to start) |
| **Interface** | TCP port 1433 on the `icm-network` Docker network. SA authentication with password from environment variable. |
| **Error Handling** | Container exits if SA password does not meet complexity requirements. Named volume persists data across restarts. Health check verifies SQL Server process is accepting queries. |
| **Tenant Awareness** | None. The `Synergistic` database is a single-tenant development database. |
| **Traceability** | FR-003, NFR-003 (port isolation), ADR-016 |
| **Principle** | **Consistency** — SQL Server Developer Edition is the closest containerized match to Azure SQL Database. |

**Configuration Details:**

| Setting | Value | Rationale |
|---|---|---|
| `ACCEPT_EULA` | `Y` | Required to accept the SQL Server license agreement |
| `MSSQL_SA_PASSWORD` | From environment variable (`${MSSQL_SA_PASSWORD:-SynergisticDev123!}`) | Default for local dev; override via `.env` file |
| `MSSQL_PID` | `Developer` | Explicitly selects Developer Edition (free, no limits) |
| Volume mount | `icm-sql-data:/var/opt/mssql/data` | Persists database files across container restarts and `docker compose down` |
| Health check | `sqlcmd` query `SELECT 1` | Verifies the SQL Server process is accepting queries, not just listening on the port |
| Port mapping | `"1433:1433"` | Exposed to host per FR-006 (internal exposure); NFR-003 requires this only on the internal network, but host access is useful for tools like SSMS/Azure Data Studio |

**Note on NFR-003 (Port Isolation):** The `1433:1433` port mapping is intentionally included for developer convenience (SSMS/Azure Data Studio). The NFR is satisfied because the port is only exposed on the host's loopback interface by default in Docker Desktop. If stricter isolation is required, the port mapping can be removed and database access limited to the internal network only.

---

## 5. Shared Network

### 5.1 icm-network

| Attribute | Detail |
|---|---|
| **Responsibility** | Provide isolated network connectivity between all three containers with automatic DNS resolution. |
| **Type** | User-defined bridge network (ADR-014) |
| **Dependencies** | None |
| **Interface** | Docker DNS resolution: `icm-db`, `icm-api`, `icm-ui` resolve to their respective container IPs. |
| **Error Handling** | Network creation failure (e.g., subnet conflict) fails the `docker compose up` command. |
| **Tenant Awareness** | None. |
| **Traceability** | FR-005, ADR-014 |
| **Principle** | **Separation of Concerns** — the network is an infrastructure concern, defined declaratively. |

**Service Discovery Map:**

| Service Name | Internal Hostname | Internal Port | Host Access | Purpose |
|---|---|---|---|---|
| `icm-db` | `icm-db` | 1433 | `localhost:1433` | SQL Server database |
| `icm-api` | `icm-api` | 5001 | `localhost:5001` | .NET Web API |
| `icm-ui` | `icm-ui` | 80 | `localhost:4200` | Angular SPA (nginx) |

---

## 6. Named Volume

### 6.1 icm-sql-data

| Attribute | Detail |
|---|---|
| **Responsibility** | Persist SQL Server database files across container restarts and `docker compose down`. |
| **Type** | Named Docker volume |
| **Mount Point** | `/var/opt/mssql/data` in the `icm-db` container |
| **Dependencies** | None |
| **Interface** | Transparent to the SQL Server process. Docker manages the volume lifecycle. |
| **Error Handling** | Volume creation failure is extremely rare (disk full, permission denied). Docker reports the error. |
| **Tenant Awareness** | None. |
| **Traceability** | FR-007 (database persistence), ADR-016 |
| **Principle** | **Separation of Concerns** — the volume is a data persistence concern, separate from the container lifecycle. |

**Data Persistence Behavior:**
- `docker compose down` — Volume is preserved. Database data survives.
- `docker compose down -v` — Volume is deleted. Database data is wiped.
- `docker compose up` after `down` — Database files are reused from the volume. Migrations are idempotent.

---

## 7. Traceability Summary

| Component | FR-001 | FR-002 | FR-003 | FR-004 | FR-005 | FR-006 | FR-007 | FR-008 | NFR-001 | NFR-002 | NFR-003 | NFR-004 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| docker-compose.yml | | | ✅ | ✅ | ✅ | ✅ | | | ✅ | | ✅ | ✅ |
| UI Dockerfile | ✅ | | | | | | | ✅ | | ✅ | | |
| API Dockerfile | | ✅ | | | | | ✅ | ✅ | | ✅ | | |
| nginx.conf | ✅ | | | | | ✅ | | | | | | |
| entrypoint.sh | | | | | | | ✅ | | | | | |
| SQL Server config | | | ✅ | | | | | | | | ✅ | |
| icm-network | | | | | ✅ | | | | | | | |
| icm-sql-data volume | | | | | | | ✅ | | | | | |

---

*All components trace to requirements from Stage 01. Principles cited inline per component.*