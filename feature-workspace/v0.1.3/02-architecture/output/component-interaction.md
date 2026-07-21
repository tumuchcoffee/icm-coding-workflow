# Component Interaction Diagram — Docker Local Development (v0.1.3)

**Feature**: Docker Local Development — Dockerfiles, Compose orchestration, and local containerized workflow
**Date**: 2026-07-20
**Version**: v0.1.3

---

## 1. System-Level Integration

This diagram shows how the v0.1.3 Docker containers map onto the established system architecture from `docs/system-architecture.md`. All three containers run locally on the developer's machine, replacing the Azure services with Docker equivalents.

```mermaid
graph TD
    subgraph "Developer Machine (Docker)"
        subgraph "Docker Network: icm-network (bridge)"
            UI["icm-ui (Angular 19 SPA)<br/>nginx:alpine<br/>Port 80 → host:4200"]
            API["icm-api (.NET 10 Web API)<br/>aspnet:10.0<br/>Port 5001 → host:5001"]
            DB["icm-db (SQL Server 2022 Dev)<br/>mssql/server:2022-latest<br/>Port 1433 → host:1433"]
        end

        VOL["icm-sql-data (Docker Volume)<br/>Database file persistence"]
    end

    subgraph "Host Machine"
        BROWSER["Browser<br/>localhost:4200"]
        POSTMAN["Postman / curl<br/>localhost:5001"]
        SSMS["SSMS / Azure Data Studio<br/>localhost:1433 (optional)"]
    end

    subgraph "Azure Infrastructure — Deferred (not in v0.1.3)"
        style AZURE fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        style FD fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        style B fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        style E fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        style G fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        style C fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        style D fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        style F fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        style H fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        style I fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        style J fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        style K fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        AZURE["Azure Cloud (Production)"]
        FD["Azure Front Door<br/>Global LB + WAF"]
        B[".NET App Service<br/>Linux Plan"]
        E["Azure SQL Database<br/>Multi-Tenant"]
        G["Azure Cache for Redis"]
        C["Azure Service Bus"]
        D["Azure Functions"]
        F["Azure Blob Storage"]
        H["Azure AD B2C / Entra ID"]
        I["Azure Key Vault"]
        J["Application Insights"]
        K["Azure Cosmos DB (Logs)"]
    end

    BROWSER -->|"HTTP :4200"| UI
    POSTMAN -->|"HTTP :5001"| API
    SSMS -.->|"SQL :1433 (optional)"| DB

    UI -->|"/api/* proxy"| API
    API -->|"TCP :1433 (internal)"| DB

    API -.->|"DbUp Migrations (startup)"| DB
    DB -->|"Read/Write"| VOL

    style UI fill:#e3f2fd,stroke:#1565c0
    style API fill:#c8e6c9,stroke:#2e7d32
    style DB fill:#fff3e0,stroke:#ef6c00
    style VOL fill:#f3e5f5,stroke:#7b1fa2
```

---

## 2. Docker Compose Orchestration View

Shows the startup dependencies, health checks, and data flows between containers as defined in `docker-compose.yml`.

```mermaid
graph TD
    subgraph "Docker Compose: icm-local"
        subgraph "Network: icm-network"
            direction TB

            subgraph "icm-db (SQL Server)"
                DB_PROCESS["SQL Server Engine<br/>Port 1433"]
                DB_HEALTH["Health Check:<br/>sqlcmd SELECT 1"]
                DB_VOL["Volume: icm-sql-data<br/>→ /var/opt/mssql/data"]
            end

            subgraph "icm-api (.NET API)"
                API_ENTRY["Entrypoint: entrypoint.sh<br/>1. Wait for DB<br/>2. Run DbUp<br/>3. Start API"]
                API_KESTREL["Kestrel Server<br/>Port 5001"]
                API_HEALTH["Health Check:<br/>curl /api/health"]
                API_DBUP["DbUp Migrations<br/>Reads: migrations/"]
            end

            subgraph "icm-ui (Angular SPA)"
                UI_NGINX["nginx:alpine<br/>Port 80"]
                UI_STATIC["Static Files<br/>dist/ → /usr/share/nginx/html"]
                UI_PROXY["Reverse Proxy<br/>/api/* → icm-api:5001"]
                UI_HEALTH["Health Check:<br/>wget /"]
            end
        end

        subgraph "Profiles"
            API_DEV["icm-api-dev (profile: dev)<br/>dotnet watch + volume mounts"]
            UI_DEV["icm-ui-dev (profile: dev)<br/>ng serve + volume mounts"]
        end
    end

    subgraph "Developer Machine"
        HOST_PORT_UI["localhost:4200"]
        HOST_PORT_API["localhost:5001"]
        HOST_PORT_DB["localhost:1433"]
        HOST_SRC["Source Code<br/>source/01-ui/src/<br/>source/02-backend/src/"]
    end

    HOST_PORT_UI --> UI_NGINX
    HOST_PORT_API --> API_KESTREL
    HOST_PORT_DB --> DB_PROCESS

    UI_PROXY --> API_KESTREL
    API_DBUP --> DB_PROCESS
    API_KESTREL --> DB_PROCESS
    DB_PROCESS --> DB_VOL

    HOST_SRC -.->|"Volume Mount (dev only)"| API_DEV
    HOST_SRC -.->|"Volume Mount (dev only)"| UI_DEV

    style DB_PROCESS fill:#fff3e0,stroke:#ef6c00
    style API_KESTREL fill:#c8e6c9,stroke:#2e7d32
    style UI_NGINX fill:#e3f2fd,stroke:#1565c0
    style API_DEV fill:#f5f5f5,stroke:#9e9e9e,stroke-dasharray: 5 5
    style UI_DEV fill:#f5f5f5,stroke:#9e9e9e,stroke-dasharray: 5 5
```

---

## 3. Dockerfile Build Pipeline

Shows the multi-stage build process for both custom images (UI and API), including layer caching strategy.

```mermaid
graph TD
    subgraph "UI Dockerfile: source/01-ui/"
        direction TB
        UI_SRC["Source Code<br/>package.json, src/"]
        UI_BUILD["Stage: build<br/>node:22-alpine<br/>npm ci → ng build"]
        UI_RUNTIME["Stage: runtime (default)<br/>nginx:alpine<br/>copy dist/ → /usr/share/nginx/html<br/>copy nginx.conf"]
        UI_DEV_STAGE["Stage: development<br/>node:22-alpine<br/>npm ci<br/>ng serve --host 0.0.0.0"]
        UI_IMAGE_PROD["icm-ui:latest<br/>< 500 MB (NFR-002)"]
        UI_IMAGE_DEV["icm-ui:dev<br/>dev server + volume mounts"]

        UI_SRC --> UI_BUILD
        UI_BUILD --> UI_RUNTIME
        UI_RUNTIME --> UI_IMAGE_PROD
        UI_SRC --> UI_DEV_STAGE
        UI_DEV_STAGE --> UI_IMAGE_DEV
    end

    subgraph "API Dockerfile: source/02-backend/"
        direction TB
        API_SRC["Source Code<br/>src/Api/, src/Application/, src/Domain/, src/Infrastructure/"]
        API_BUILD["Stage: build<br/>dotnet/sdk:10.0<br/>dotnet restore → dotnet publish"]
        API_RUNTIME["Stage: runtime (default)<br/>dotnet/aspnet:10.0<br/>copy publish/ + entrypoint.sh + migrations/"]
        API_DEV_STAGE["Stage: development<br/>dotnet/sdk:10.0<br/>entrypoint.sh watch"]
        API_IMAGE_PROD["icm-api:latest<br/>< 300 MB (NFR-002)"]
        API_IMAGE_DEV["icm-api:dev<br/>dotnet watch + volume mounts"]

        API_SRC --> API_BUILD
        API_BUILD --> API_RUNTIME
        API_RUNTIME --> API_IMAGE_PROD
        API_SRC --> API_DEV_STAGE
        API_DEV_STAGE --> API_IMAGE_DEV
    end

    style UI_BUILD fill:#fff9c4,stroke:#f9a825
    style UI_RUNTIME fill:#e3f2fd,stroke:#1565c0
    style UI_DEV_STAGE fill:#f5f5f5,stroke:#9e9e9e,stroke-dasharray: 5 5
    style API_BUILD fill:#fff9c4,stroke:#f9a825
    style API_RUNTIME fill:#c8e6c9,stroke:#2e7d32
    style API_DEV_STAGE fill:#f5f5f5,stroke:#9e9e9e,stroke-dasharray: 5 5
```

---

## 4. .NET Solution — Layer Dependencies (Docker Context)

Shows how the Clean Architecture layers map into the Docker container. The key difference from v0.1.2 is that the API runs inside a container with DbUp and an entrypoint script.

```mermaid
graph TD
    subgraph "icm-api Container (Docker)"
        subgraph "Entrypoint Layer"
            ENTRY["entrypoint.sh<br/>Wait → Migrate → Start"]
        end

        subgraph "Api Layer"
            API_CONTROLLERS["Controllers/<br/>HealthController.cs"]
            API_PROGRAM["Program.cs<br/>AddControllers()<br/>MapControllers()"]
        end

        subgraph "Application Layer"
            APP_HEALTH["Features/Health/<br/>HealthCheckResponse.cs"]
            APP_DI["DependencyInjection.cs<br/>(placeholder)"]
        end

        subgraph "Domain Layer"
            DOM["Domain.csproj<br/>(placeholder)"]
        end

        subgraph "Infrastructure Layer"
            INF_DI["DependencyInjection.cs<br/>(placeholder)"]
            INF_MIGRATIONS["Migrations/<br/>001_CreateSchemaVersion.sql<br/>(embedded/copied)"]
        end
    end

    subgraph "External (Docker Network)"
        DB["icm-db<br/>SQL Server 2022<br/>Synergistic database"]
    end

    ENTRY --> API_PROGRAM
    API_PROGRAM --> API_CONTROLLERS
    API_CONTROLLERS --> APP_HEALTH
    API_PROGRAM --> APP_DI
    API_PROGRAM --> INF_DI
    APP_DI --> DOM
    INF_DI --> APP_DI

    ENTRY -.->|"DbUp reads"| INF_MIGRATIONS
    ENTRY -.->|"DbUp applies to"| DB
    API_CONTROLLERS -.->|"Future: Dapper queries"| DB

    style ENTRY fill:#fff9c4,stroke:#f9a825
    style API_CONTROLLERS fill:#c8e6c9,stroke:#2e7d32
    style API_PROGRAM fill:#c8e6c9,stroke:#2e7d32
    style APP_HEALTH fill:#c8e6c9,stroke:#2e7d32
    style DOM fill:#f5f5f5,stroke:#9e9e9e,stroke-dasharray: 5 5
    style INF_DI fill:#f5f5f5,stroke:#9e9e9e,stroke-dasharray: 5 5
    style DB fill:#fff3e0,stroke:#ef6c00
```

---

## 5. Network Traffic Flow — Request Paths

Shows the three primary request paths through the Docker stack.

```mermaid
graph LR
    subgraph "Host Machine"
        BROWSER["Browser<br/>localhost:4200"]
    end

    subgraph "Docker: icm-network"
        NGINX["icm-ui nginx<br/>:80"]
        KESTREL["icm-api Kestrel<br/>:5001"]
        SQL["icm-db SQL Server<br/>:1433"]
    end

    BROWSER -->|"1. GET /<br/>(Static SPA file)"| NGINX
    NGINX -->|"Serve index.html<br/>or static asset"| BROWSER

    BROWSER -->|"2. GET /api/health<br/>(API proxy)"| NGINX
    NGINX -->|"proxy_pass<br/>http://icm-api:5001"| KESTREL
    KESTREL -->|"200 OK"| NGINX
    NGINX -->|"200 OK"| BROWSER

    BROWSER -->|"3. GET /api/tenants<br/>(API → DB)"| NGINX
    NGINX -->|"proxy_pass"| KESTREL
    KESTREL -->|"Dapper query<br/>(future)"| SQL
    SQL -->|"Result set"| KESTREL
    KESTREL -->|"JSON response"| NGINX
    NGINX -->|"JSON response"| BROWSER

    style BROWSER fill:#e8eaf6,stroke:#3f51b5
    style NGINX fill:#e3f2fd,stroke:#1565c0
    style KESTREL fill:#c8e6c9,stroke:#2e7d32
    style SQL fill:#fff3e0,stroke:#ef6c00
```

**Key observations:**
- **Path 1** (Static files): Browser → nginx → Browser. No API involvement. Fast, cached responses.
- **Path 2** (API health): Browser → nginx → API → nginx → Browser. API is reached via nginx proxy, not directly. This mirrors the production Front Door pattern.
- **Path 3** (API with data): Browser → nginx → API → SQL → API → nginx → Browser. Full round-trip through every layer. This is the production-equivalent data flow.

---

## 6. Environment Comparison — Local Docker vs. Production Azure

```mermaid
graph TD
    subgraph "Local Development (v0.1.3)"
        direction TB
        L_BROWSER["Browser<br/>localhost:4200"]
        L_NGINX["nginx:alpine<br/>(icm-ui container)"]
        L_API["aspnet:10.0<br/>(icm-api container)"]
        L_DB["mssql/server:2022-latest<br/>(icm-db container)"]
        L_VOL["Docker Volume<br/>icm-sql-data"]

        L_BROWSER --> L_NGINX
        L_NGINX --> L_API
        L_API --> L_DB
        L_DB --> L_VOL
    end

    subgraph "Production (Azure)"
        direction TB
        P_BROWSER["Browser<br/>icm.example.com"]
        P_FD["Azure Front Door<br/>WAF + CDN"]
        P_AS["App Service (.NET API)<br/>Linux P1v3"]
        P_SQL["Azure SQL Database<br/>S3, Geo-Replicated"]
        P_KV["Key Vault<br/>Connection Strings"]

        P_BROWSER --> P_FD
        P_FD --> P_AS
        P_AS --> P_SQL
        P_AS --> P_KV
    end

    L_NGINX -.->|"Maps to"| P_FD
    L_API -.->|"Maps to"| P_AS
    L_DB -.->|"Maps to"| P_SQL

    style L_NGINX fill:#e3f2fd,stroke:#1565c0
    style L_API fill:#c8e6c9,stroke:#2e7d32
    style L_DB fill:#fff3e0,stroke:#ef6c00
    style P_FD fill:#e3f2fd,stroke:#1565c0
    style P_AS fill:#c8e6c9,stroke:#2e7d32
    style P_SQL fill:#fff3e0,stroke:#ef6c00
    style P_KV fill:#f3e5f5,stroke:#7b1fa2
```

---

## 7. File System Map — Repository to Docker Context

Shows which files are copied into which container stages.

```mermaid
graph TD
    subgraph "Repository Root"
        DOCKER_COMPOSE["docker-compose.yml"]
        SOURCE_UI["source/01-ui/"]
        SOURCE_API["source/02-backend/"]
        SOURCE_SQL["source/03-sql/"]
    end

    subgraph "icm-ui Build Context (source/01-ui/)"
        UI_DOCKERFILE["Dockerfile"]
        UI_PKG["package.json"]
        UI_SRC["src/"]
        UI_NGINX_CONF["nginx.conf"]
    end

    subgraph "icm-api Build Context (source/02-backend/)"
        API_DOCKERFILE["Dockerfile"]
        API_ENTRY["entrypoint.sh"]
        API_SRC["src/ (Api, Application, Domain, Infrastructure)"]
    end

    subgraph "icm-ui Container (runtime)"
        UI_DIST["/usr/share/nginx/html/<br/>(from build stage)"]
        UI_CONF["/etc/nginx/conf.d/default.conf<br/>(from nginx.conf)"]
    end

    subgraph "icm-api Container (runtime)"
        API_PUBLISH["/app/publish/<br/>(from build stage)"]
        API_MIGRATIONS["/app/migrations/<br/>(from source/03-sql/migrations/)"]
        API_ENTRY_SCRIPT["/app/entrypoint.sh"]
    end

    DOCKER_COMPOSE --> UI_DOCKERFILE
    DOCKER_COMPOSE --> API_DOCKERFILE

    SOURCE_UI --> UI_DOCKERFILE
    UI_DOCKERFILE --> UI_DIST
    UI_DOCKERFILE --> UI_CONF
    UI_NGINX_CONF --> UI_CONF

    SOURCE_API --> API_DOCKERFILE
    API_DOCKERFILE --> API_PUBLISH
    API_DOCKERFILE --> API_ENTRY_SCRIPT
    API_ENTRY --> API_ENTRY_SCRIPT

    SOURCE_SQL --> API_MIGRATIONS

    style DOCKER_COMPOSE fill:#fff9c4,stroke:#f9a825
    style UI_DIST fill:#e3f2fd,stroke:#1565c0
    style UI_CONF fill:#e3f2fd,stroke:#1565c0
    style API_PUBLISH fill:#c8e6c9,stroke:#2e7d32
    style API_MIGRATIONS fill:#c8e6c9,stroke:#2e7d32
    style API_ENTRY_SCRIPT fill:#c8e6c9,stroke:#2e7d32
```

---

## 8. Component Traceability Matrix

| Diagram | FR-001 | FR-002 | FR-003 | FR-004 | FR-005 | FR-006 | FR-007 | FR-008 | NFR-001 | NFR-002 | NFR-003 | NFR-004 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| System-Level Integration | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | | | | | ✅ | |
| Docker Compose Orchestration | | | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | | | | |
| Dockerfile Build Pipeline | ✅ | ✅ | | | | | | | | ✅ | | |
| .NET Solution (Docker Context) | | ✅ | | | | | ✅ | | | | | |
| Network Traffic Flow | ✅ | ✅ | | | | ✅ | | | | | | |
| Environment Comparison | ✅ | ✅ | ✅ | | | | | | | | | |
| File System Map | ✅ | ✅ | ✅ | | | | ✅ | | | | | |

---

*All diagrams trace to functional and non-functional requirements from Stage 01. The established system architecture diagram style from `docs/system-architecture.md` is used throughout.*