# System Architecture

## Document Purpose

This document defines the system-level architecture for the ICM SaaS platform — a multi-tenant application built with **Angular** (frontend), **.NET 10 Web API** (backend), and **SQL Server** (data), hosted on **Microsoft Azure**. It establishes architectural principles, component boundaries, technology decisions, and operational patterns that guide all development efforts.

---

## 1. Architecture Principles

| Principle | Description |
|---|---|
| **Separation of Concerns** | UI, business logic, and data access are cleanly separated across layers and projects. |
| **Multi-Tenancy by Design** | Every layer — from the database to the UI — is tenant-aware. Tenant isolation uses a shared-database, row-level model with `TenantId` partitioning. |
| **API-First** | All business capabilities are exposed through versioned REST APIs. The Angular SPA consumes only the public API surface. |
| **Infrastructure as Code** | All Azure resources are provisioned via Bicep. No manual portal changes in production. |
| **Security in Depth** | Defense at every layer: network, identity, application, and data. |
| **Observability** | Structured logging, distributed tracing, and metrics are built in from day one. |
| **Cost-Aware** | Resources are right-sized. Auto-scale rules keep costs proportional to usage. |
| **Resilience** | Transient failures are handled with retry policies, circuit breakers, and graceful degradation. |

---

## 2. High-Level Architecture

```mermaid
graph TD
    subgraph "Client Layer"
        A[Angular SPA<br/>Static Web Apps / CDN]
    end

    subgraph "Azure Front Door / CDN"
        FD[Azure Front Door<br/>Global Load Balancer + WAF]
    end

    subgraph "API Layer"
        B[Azure App Service<br/>.NET 10 Web API<br/>Linux Plan]
    end

    subgraph "Integration & Events"
        C[Azure Service Bus<br/>Topics & Queues]
        D[Azure Functions<br/>Background Workers]
    end

    subgraph "Data Layer"
        E[(Azure SQL Database<br/>Multi-Tenant)]
        F[Azure Blob Storage<br/>Unstructured Data]
        G[Azure Cache for Redis<br/>Session & Query Cache]
    end

    subgraph "Identity & Security"
        H[Azure AD B2C / Entra ID<br/>Auth & MFA]
        I[Azure Key Vault<br/>Secrets & Certificates]
    end

    subgraph "Observability"
        J[Application Insights<br/>Metrics &amp; Tracing]
        K[(Azure Cosmos DB for NoSQL<br/>Structured Log Store)]
    end

    FD --> A
    A -->|HTTPS / REST| FD
    FD --> B
    B --> E
    B --> G
    B --> C
    C --> D
    D --> E
    D --> F
    B --> I
    D --> I
    B -->|Telemetry| J
    D -->|Telemetry| J
    A -->|Telemetry| J
    B -->|Logs| K
    D -->|Logs| K
    H -->|Auth Tokens| A
    H -->|Token Validation| B
```

---

## 3. Frontend Architecture — Angular SPA

### 3.1 Technology Stack

| Concern | Technology |
|---|---|
| Framework | Angular 19+ (standalone components) |
| Language | TypeScript 5.x, strict mode |
| State Management | NgRx SignalStore |
| UI Component Library | PrimeNG + Angular CDK |
| HTTP Client | Angular `HttpClient` with interceptors |
| Build | Vite (via Angular CLI / esbuild) |
| Testing | Jest (unit), Playwright (E2E) |

### 3.2 Project Structure (Feature-Based)

```
src/
├── app/
│   ├── core/                 # Singleton services, guards, interceptors
│   │   ├── auth/
│   │   ├── http/
│   │   └── logging/
│   ├── shared/               # Shared components, directives, pipes
│   │   ├── components/
│   │   ├── directives/
│   │   └── pipes/
│   ├── features/             # Lazy-loaded feature modules
│   │   ├── dashboard/
│   │   ├── tenants/
│   │   ├── users/
│   │   └── billing/
│   ├── layout/               # Shell, nav, footer
│   │   ├── main-layout/
│   │   └── auth-layout/
│   └── app.config.ts         # Standalone bootstrap configuration
├── environments/
└── assets/
```

### 3.3 Key Patterns

- **Standalone Components**: No `NgModule`. Every component, pipe, and directive is standalone.
- **Separate Template & Style Files**: Every component uses `templateUrl` and `styleUrls` (pointing to `.component.html` and `.component.scss` files) rather than inline `template` and `styles`. This keeps TypeScript logic, HTML structure, and SCSS styling in dedicated files for clarity and maintainability.
- **Signals for State**: `signal()`, `computed()`, and `effect()` for local state; NgRx SignalStore for feature-level state.
- **Lazy Loading**: Every feature is loaded lazily via the Angular Router.
- **HTTP Interceptors**: Centralized auth token injection, tenant header injection, error normalization, and telemetry correlation.
- **Tenant Context**: The current tenant is resolved on app load (from subdomain or path) and provided globally.

### 3.4 API Communication

| Header | Purpose |
|---|---|
| `Authorization: Bearer <token>` | JWT from Azure AD B2C / Entra ID |
| `X-Tenant-Id: <guid>` | Tenant context for every request |
| `X-Correlation-Id: <guid>` | Distributed tracing correlation |
| `Accept: application/json` | Content negotiation |
| `x-api-version: 2025-01-01` | API version routing |

---

## 4. Backend Architecture — .NET 10 Web API

### 4.1 Technology Stack

| Concern | Technology |
|---|---|
| Runtime | .NET 10 (LTS) |
| API Framework | ASP.NET Core Controllers |
| ORM | Dapper |
| Validation | FluentValidation |
| Background Jobs | Azure Functions (isolated process) |
| Messaging | Azure Service Bus + MassTransit |
| Caching | Azure Cache for Redis + `IDistributedCache` |
| Logging | Serilog → Azure Cosmos DB for NoSQL |
| Resilience | Polly + `Microsoft.Extensions.Http.Resilience` |
| Testing | xUnit, NSubstitute, Testcontainers |

### 4.2 Solution Structure

```
src/
├── Api/                          # ASP.NET Core host, middleware, controllers
│   ├── Controllers/              # API controllers (by feature)
│   ├── Middleware/
│   └── Program.cs
├── Application/                  # Use cases, commands, queries, DTOs
│   ├── Features/
│   │   ├── Tenants/
│   │   ├── Users/
│   │   └── Billing/
│   ├── Common/
│   │   ├── Behaviors/            # MediatR pipeline behaviors
│   │   └── Interfaces/
│   └── DependencyInjection.cs
├── Domain/                       # Entities, value objects, domain events
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Events/
│   └── Exceptions/
├── Infrastructure/               # Dapper, external services, messaging
│   ├── Persistence/
│   │   ├── Sql/                   # SQL scripts, stored procedures, migrations
│   │   └── Migrations/
│   ├── Services/
│   └── Messaging/
└── Contracts/                    # Shared DTOs for inter-service communication
```

### 4.3 Clean Architecture with Controllers

The backend follows **Clean Architecture** (onion/hexagonal). Dependencies point inward:

```mermaid
graph LR
    A[Api] --> B[Application]
    A --> C[Infrastructure]
    B --> D[Domain]
    C --> B
```

- **Domain** — Zero external dependencies. Pure entities, value objects, and domain events.
- **Application** — Use cases orchestrated via MediatR. Depends only on Domain.
- **Infrastructure** — Dapper `SqlConnection` wrappers, Azure Service Bus clients, blob storage, email services. Implements interfaces defined in Application.
- **Api** — Thin shell. Maps controllers, wires middleware, registers services.

### 4.4 Request Pipeline (Typical Flow)

```mermaid
sequenceDiagram
    participant Client as Angular SPA
    participant APIM as Azure Front Door
    participant API as .NET API
    participant Mediatr as MediatR
    participant DB as SQL Database
    participant Cache as Redis Cache
    participant SB as Service Bus

    Client->>APIM: HTTPS Request
    APIM->>API: Forward (WAF validated)
    API->>API: Auth Middleware (Validate JWT)
    API->>API: Tenant Resolution Middleware
    API->>API: Correlation Middleware
    API->>Mediatr: Send Command/Query
    Mediatr->>Mediatr: Validation Pipeline
    Mediatr->>Mediatr: Authorization Pipeline
    Mediatr->>Cache: Check Cache (queries)
    alt Cache Miss
        Mediatr->>DB: Execute Query (Dapper)
        Mediatr->>Cache: Populate Cache
    end
    Mediatr-->>API: Response
    alt Domain Event Raised
        API->>SB: Publish Integration Event
    end
    API-->>Client: JSON Response
```

### 4.5 Controller Pattern

```csharp
// Example: Api/Controllers/TenantsController.cs
[ApiController]
[Route("api/tenants")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TenantDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTenants(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTenantsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenantById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTenantByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
```

### 4.6 Multi-Tenant Strategy

- **Row-Level Tenancy**: Every table includes a `TenantId` column (non-nullable `uniqueidentifier`).
- **Query Isolation**: Every SQL query and stored procedure includes a `WHERE TenantId = @TenantId` clause. The repository layer enforces this convention.
- **Tenant Resolution**: Middleware extracts `X-Tenant-Id` header → validates user belongs to tenant → sets `ITenantContext.TenantId` → injected into repository calls via `SqlConnection` extension.
- **Data Isolation**: Enforced at the query level; mandatory tenant filter in every repository method. No raw SQL elsewhere that bypasses the filter.

---

## 5. Data Architecture — SQL Server

### 5.1 Database Design Principles

- **Schema ownership**: All application objects reside in `dbo` (or dedicated `app` schema).
- **Migrations**: All schema changes are managed through versioned SQL migration scripts (e.g., DbUp or FluentMigrator) — the source of truth for database state. Scripts are idempotent and run as part of CI/CD deployment.
- **Row-Level Security (RLS)**: Supplementary guard in the database layer — predicates match the `TenantId` filter.
- **Idempotent Scripts**: All manual SQL scripts check for existence before creating or altering.

### 5.2 Indexing Strategy

| Index Type | When to Use |
|---|---|
| Clustered Index | Primary key. Defaults to `Id` (`uniqueidentifier`, non-sequential — use `NEWSEQUENTIALID()` or prefer `int`/`bigint` for PK, use GUID only for external reference). |
| Non-Clustered Index | `TenantId`, foreign keys, columns in `WHERE`/`JOIN`/`ORDER BY`. |
| Filtered Index | Columns with well-known subsets (e.g., `WHERE IsDeleted = 0`). |
| Columnstore Index | Large fact/reporting tables. |

### 5.3 Query Performance

- Use Dapper's buffered queries by default; use `QueryAsync<T>(..., buffered: false)` for large result sets to stream rows without loading them all into memory.
- Prefer pagination with keyset (seek) pagination over `OFFSET/FETCH` for large datasets.
- Use `Query Store` enabled on Azure SQL Database for query performance insights.
- Use stored procedures for complex multi-statement operations to reduce round-trips and benefit from cached execution plans.

### 5.4 Backup & Retention

- **Azure SQL Database**: Automated backups (7–35 day retention, configurable per environment).
- **Long-Term Retention (LTR)**: Weekly/monthly/yearly backups for compliance; stored in geo-redundant storage.
- **Geo-Replication**: Active geo-replication to a paired region for disaster recovery.

---

## 6. Azure Infrastructure

### 6.1 Resource Map

| Resource | SKU (Minimum) | Purpose |
|---|---|---|
| Azure Front Door | Standard | Global load balancing, WAF, CDN |
| App Service Plan | P1v3 (Linux) | .NET API hosting |
| Azure SQL Database | S3 (50 DTU) | Primary transactional database |
| Azure Cache for Redis | C1 (1 GB) | Session state, response caching |
| Azure Service Bus | Standard | Async messaging between services |
| Azure Functions | Consumption | Background workers |
| Azure Blob Storage | Hot/Cool (LRS) | File uploads, static assets |
| Azure Key Vault | Standard | Secrets, connection strings, certificates |
| Application Insights | Workspace-based | Metrics, distributed tracing, alerting |
| Azure Cosmos DB for NoSQL | Serverless (or Autoscale 400 RU/s) | Structured log store |
| Azure AD B2C / Entra ID | — | Authentication and user management |

### 6.2 Environments

| Environment | Purpose | Naming Convention |
|---|---|---|
| `dev` | Development & integration testing | `{app}-{env}-{region}-{resource}` |
| `staging` | Pre-production validation | `{app}-stg-{region}-{resource}` |
| `prod` | Production | `{app}-prd-{region}-{resource}` |

> Example: `icm-prd-eastus-asp` (App Service Plan, Production, East US)

### 6.3 Networking

- **VNet Integration**: App Service integrated with a dedicated VNet (outbound traffic restricted).
- **Private Endpoints**: SQL Database, Storage Account, Cosmos DB, and Key Vault use Private Link (no public exposure).
- **Service Endpoints**: Redis and Service Bus accessible via VNet service endpoints.
- **Azure Front Door WAF**: OWASP Top 10 rule set, rate limiting, IP filtering at the edge.

### 6.4 Scaling Rules

| Resource | Scale Rule |
|---|---|
| App Service | CPU > 70% for 5 min → +1 instance (max 10). Memory-based rules as secondary. |
| Azure SQL | Auto-scale to next tier when DTU > 80% sustained for 10 min. |
| Azure Functions | Event-driven (Service Bus queue depth, blob triggers). |
| Azure Cosmos DB | Autoscale mode — scales RU/s within configured min/max range based on load. |

---

## 7. Security Architecture

### 7.1 Authentication & Authorization

```
[Angular SPA]
    │
    ▼
[Azure AD B2C / Entra ID]
    ├── Sign-up / Sign-in (with MFA)
    ├── Token Issuance (JWT — access + refresh tokens)
    │
    ▼
[.NET API]
    ├── JWT Bearer Authentication Middleware
    ├── Tenant Resolution Middleware
    ├── Role-Based Authorization (`[Authorize(Roles = "Admin")]`)
    └── Resource-Based Authorization (IAuthorizationHandler)
```

### 7.2 Secret Management

- **Zero secrets in code or config files.**
- All connection strings, API keys, and certificates stored in **Azure Key Vault**.
- App Service and Functions authenticate to Key Vault via **Managed Identity**.
- Key Vault references in App Service app settings: `@Microsoft.KeyVault(SecretUri=https://icm-prd-kv.vault.azure.net/secrets/ConnectionStrings--Default/)`.
- Key rotation follows a 90-day policy with automated renewal.

### 7.3 Data Protection

- **TLS 1.3** enforced for all ingress traffic (Front Door → App Service).
- **Encryption at Rest**: Azure SQL (TDE), Cosmos DB (service-managed keys), Blob Storage (service-managed keys).
- **Encryption in Transit**: HTTPS enforced everywhere; SQL connections use TLS.
- **Column-Level Encryption**: `Always Encrypted` for PII and sensitive financial data.
- **Data Masking**: Dynamic data masking on `Email` and `Phone` columns in non-production environments shared with developers.

### 7.4 OWASP Mitigations

| Threat | Mitigation |
|---|---|
| SQL Injection | Dapper parameterized queries — all user input bound via `@param`; validate all inputs |
| XSS | Angular's built-in sanitization; Content-Security-Policy header |
| CSRF | SameSite=Strict cookies; token-based auth (JWT in Authorization header) |
| Broken Access Control | Tenant-aware authorization handlers; row-level security in SQL |
| Sensitive Data Exposure | Always Encrypted; data masking; HTTPS everywhere |

---

## 8. DevOps & CI/CD

### 8.1 Branching Strategy

```
main          ← Production. Deploy to prod environment.
  └── release ← Staging. Deploy to staging environment.
        └── feature/* ← Active development. Merge to release via PR.
```

- Trunk-based development with short-lived feature branches.
- All merges require at least one peer review.
- Branch protection rules on `main` and `release`.

### 8.2 CI/CD Pipeline (GitHub Actions / Azure DevOps)

```mermaid
graph LR
    A[Push / PR] --> B[Build & Test]
    B --> C{Pass?}
    C -->|Yes| D[Publish Artifacts]
    C -->|No| E[Fail - Notify]
    D --> F{Target Branch}
    F -->|feature/*| G[Stop]
    F -->|release| H[Deploy to Staging]
    F -->|main| I[Deploy to Production]
    H --> J[Run Integration Tests]
    J --> K[Approval Gate]
    K --> I
```

### 8.3 Pipeline Steps

1. **Restore** — NuGet + npm dependencies
2. **Lint** — ESLint for Angular, Roslyn analyzers for .NET
3. **Unit Tests** — Jest (frontend), xUnit (backend)
4. **Build** — `dotnet publish` (self-contained Linux), `ng build` (AOT + optimization)
5. **Containerize** — Docker image from multi-stage build
6. **Security Scan** — Container vulnerability scan, dependency audit
7. **Deploy** — Swap deployment to staging slot, smoke tests, then production swap
8. **Post-Deployment** — Run SQL migrations (DbUp/FluentMigrator), warm cache

---

## 9. Observability

### 9.1 Three Pillars

| Pillar | Tool | What We Track |
|---|---|---|
| **Logging** | Serilog → Azure Cosmos DB for NoSQL | Structured logs with `TenantId`, `CorrelationId`, `UserId`, `Severity`, `Source` enrichment |
| **Metrics** | Application Insights / Azure Monitor | Request rate, latency (p50/p95/p99), error rate, DTU %, cache hit ratio |
| **Tracing** | Application Insights / OpenTelemetry | End-to-end request flow: Front Door → API → SQL → Service Bus → Function |

### 9.2 Log Storage — Cosmos DB for NoSQL

Structured application logs are written directly to Cosmos DB via the **Serilog Cosmos DB sink**, bypassing Application Insights' log ingestion. This provides:

- **Schema flexibility**: Log documents can evolve without migrations — add new enrichment properties at any time.
- **Cost efficiency**: Pay per-RU rather than per-GB ingested. App Insights log ingestion is expensive at scale; reserving it for metrics and traces lowers cost.
- **Queryable**: Full SQL-like queries over JSON documents — filter by tenant, correlation, severity, or time range.
- **TTL policies**: Set per-container `ttl` to auto-expire verbose/debug logs after N days while retaining error and audit logs longer.
- **Partition strategy**: Partition by `TenantId` for tenant-scoped queries and even throughput distribution.

#### Cosmos DB Log Container Design

```
Container: application-logs
Partition Key: /TenantId
```

**Log document schema:**

```json
{
    "id": "guid",
    "Timestamp": "2026-07-17T14:30:00Z",
    "Level": "Error",
    "MessageTemplate": "Failed to provision tenant {TenantId}",
    "Message": "Failed to provision tenant abc-123",
    "Exception": "...",
    "Properties": {
        "TenantId": "abc-123",
        "UserId": "user-456",
        "CorrelationId": "corr-789",
        "SourceContext": "Api.Controllers.Tenants",
        "MachineName": "icm-prd-api-01",
        "DurationMs": 3421
    }
}
```

#### Serilog Configuration

```csharp
// Program.cs — Cosmos DB log sink
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ICM-API")
    .Enrich.WithMachineName()
    .WriteTo.AzureCosmosDB(
        endpointUri: builder.Configuration["Logging:CosmosDb:EndpointUri"]!,
        key: builder.Configuration["Logging:CosmosDb:Key"]!,       // from Key Vault
        databaseName: "Observability",
        containerName: "application-logs",
        partitionKey: "TenantId",
        timeToLive: TimeSpan.FromDays(30),                          // auto-expire after 30 days
        storeTimestampInUtc: true)
    .CreateLogger();
```

### 9.3 Correlation Between Logs, Metrics, and Traces

While logs live in Cosmos DB, **Application Insights** handles metrics and distributed tracing. The `CorrelationId` and `OperationId` tie them together:

```mermaid
sequenceDiagram
    participant Logs as Cosmos DB Logs
    participant Traces as App Insights Traces
    participant Metrics as App Insights Metrics

    Note over Logs, Metrics: All share CorrelationId + OperationId
    Logs->>Logs: "Error: Provisioning failed" (CorrelationId: corr-789)
    Traces->>Traces: Request dependency chain (OperationId: op-456)
    Metrics->>Metrics: Error rate spike at 14:30 (correlated to OperationId)
```

### 9.4 Log Retention Policy

| Log Level | TTL | Rationale |
|---|---|---|
| `Verbose` / `Debug` | 7 days | High volume, low value after immediate debugging |
| `Information` | 30 days | Operational visibility for recent history |
| `Warning` | 90 days | Pattern detection, trend analysis |
| `Error` / `Fatal` | 365 days | Compliance, post-mortems, audit trail |

Managed via Cosmos DB's built-in `ttl` property. Set per-level by writing to separate containers or using a `ttl` field on each document based on `Level`.

### 9.5 Alerting

| Alert | Threshold | Severity |
|---|---|---|
| API Error Rate | > 5% for 5 min | Critical |
| API Latency (p95) | > 2000 ms for 5 min | Warning |
| SQL DTU | > 90% for 10 min | Warning |
| Cosmos DB RU Consumption | > 80% of provisioned for 10 min | Warning |
| Failed Login Attempts | > 50 in 5 min | Critical |
| Service Bus Dead-Letter | > 10 messages in queue | Warning |
| Certificate Expiry | Within 14 days | Critical |
| Log Ingestion Lag | Cosmos DB writes > 5s behind | Warning |

---

## 10. SaaS Operational Practices

### 10.1 Tenant Lifecycle

1. **Provisioning** — Azure Function triggers on new tenant creation: initializes tenant schema seed data, default roles, storage container, and baseline configuration.
2. **Configuration** — Per-tenant settings stored in a JSON column (`Tenant.Configurations`) for flexibility without schema changes.
3. **Deprovisioning** — Soft-delete with retention period (30 days) before hard delete. Data export available via API.

### 10.2 Billing & Metering

- Usage events published to Service Bus → Function aggregates into billing records.
- Metered dimensions: active users, API calls, storage used.
- Billing data stored in a dedicated `Billing` schema for reporting.

### 10.3 Rate Limiting & Throttling

- **Per-Tenant Rate Limiting**: ASP.NET Core rate limiter middleware.
- Fixed window: 1000 requests/min per tenant (configurable per tier).
- **429 Too Many Requests** returned with `Retry-After` header.

### 10.4 Feature Flags

- **Azure App Configuration** for feature flags.
- Gradual rollout by tenant or user percentage.
- No code deployment needed to toggle features.

---

## 11. Disaster Recovery

| Scenario | RPO | RTO | Strategy |
|---|---|---|---|
| Region outage | 5 min | 30 min | Active geo-replication (SQL), Front Door failover to paired region |
| Accidental data deletion | Near-zero | 1 hour | Point-in-time restore from automated backups |
| Corrupt deployment | N/A | 5 min | Deployment slot swap-back |
| Ransomware | Near-zero | 2 hours | Immutable blob storage backups, isolated recovery environment |

---

## 12. Local Development with Docker

### 12.1 Overview

All local development and testing runs through Docker Compose, providing a consistent, isolated environment that mirrors production without requiring local installation of .NET, Node.js, or SQL Server. The compose stack includes **hot reload** on both the frontend and backend, and **SQL data persists across container restarts** via a named Docker volume — no data loss on `docker compose down` or `docker compose stop` / `start`.

```mermaid
graph TD
    subgraph "Docker Host (localhost)"
        UI[icm-ui<br/>Angular Dev Server<br/>:4200]
        API[icm-api<br/>.NET 10 Web API<br/>:5001]
        SQL[(icm-db<br/>SQL Server 2022<br/>:1433)]
    end

    subgraph "External Tools"
        Browser[Browser<br/>localhost:4200]
        SSMS[SSMS / Azure Data Studio<br/>localhost:1433]
    end

    Browser -->|HTTP| UI
    UI -->|/api proxy| API
    API -->|SQL| SQL
    SSMS -->|SQL Auth| SQL
```

### 12.2 Prerequisites

| Tool | Version | Purpose |
|---|---|---|
| Docker Desktop | Latest stable | Container runtime & Compose |
| A SQL client | SSMS, Azure Data Studio, or `sqlcmd` | Inspect persistent data |

No local installation of .NET SDK, Node.js, or SQL Server is required.

### 12.3 File Layout

All Docker assets live at the repository root:

```
source/
├── .dockerignore              # Shared exclude rules for all Dockerfiles
├── docker-compose.yml          # Full local stack definition
├── docker/
│   ├── api/
│   │   └── Dockerfile          # Multi-stage .NET 10 build
│   ├── ui/
│   │   └── Dockerfile          # Multi-stage Angular 19 dev build
│   └── db/
│       └── entrypoint.sh       # SQL Server startup + migration runner
└── ...
```

### 12.4 SQL Server Container & Data Persistence

The SQL Server container uses a **named Docker volume** (`sql-data`) mounted at `/var/opt/mssql/data`. This volume survives:

- `docker compose stop` / `docker compose start` (container restart)
- `docker compose down` (container removal)
- Docker Desktop restarts and system reboots

The volume is only destroyed by an explicit `docker compose down -v`, which removes all named volumes. Normal development workflows never trigger this.

##### Connecting from SSMS / Azure Data Studio

| Property | Value |
|---|---|
| Server name | `localhost,1433` |
| Authentication | SQL Server Authentication |
| Login | `sa` |
| Password | Set via `.env` (see below) — default: `DevPassword123!` |
| Encrypt connection | Optional (set to `False` for local dev) |

All databases and tables are visible, queryable, and editable through SSMS just like a local SQL Server instance.

### 12.5 `docker-compose.yml`

```yaml
# docker-compose.yml — local development stack
# Start:  docker compose up -d
# Stop:   docker compose stop      (preserves volumes & data)
# Down:   docker compose down      (removes containers, preserves volumes)
# Nuke:   docker compose down -v   (removes containers AND volumes — data lost)

name: icm-dev

services:
  # ── SQL Server 2022 ────────────────────────────────────────────
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: icm-db
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: ${SA_PASSWORD:-DevPassword123!}
      MSSQL_PID: Developer
    ports:
      - "1433:1433"
    volumes:
      - sql-data:/var/opt/mssql/data                  # Persistent database files
      - ./source/03-sql/migrations:/migrations:ro     # Read-only migration scripts
      - ./docker/db/entrypoint.sh:/entrypoint.sh:ro   # Startup script
    healthcheck:
      test: /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$${MSSQL_SA_PASSWORD}" -Q "SELECT 1" || exit 1
      interval: 10s
      timeout: 5s
      retries: 10
      start_period: 30s
    restart: unless-stopped

  # ── .NET 10 Web API (hot reload) ────────────────────────────────
  api:
    build:
      context: ./source/02-backend
      dockerfile: ../../docker/api/Dockerfile
      target: dev
    container_name: icm-api
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5001
      - ConnectionStrings__Default=Server=db,1433;Database=icm-dev;User Id=sa;Password=${SA_PASSWORD:-DevPassword123!};TrustServerCertificate=True;
    ports:
      - "5001:5001"
    volumes:
      - ./source/02-backend/src:/app/src:consistent   # Hot reload watches source
    depends_on:
      db:
        condition: service_healthy
    restart: unless-stopped

  # ── Angular SPA (dev server with hot reload) ───────────────────
  ui:
    build:
      context: ./source/01-ui
      dockerfile: ../../docker/ui/Dockerfile
      target: dev
    container_name: icm-ui
    ports:
      - "4200:4200"
    volumes:
      - ./source/01-ui/src:/app/src:consistent   # Hot reload watches source
      - ui-node-modules:/app/node_modules        # Anonymous volume for deps
    depends_on:
      - api
    restart: unless-stopped

volumes:
  sql-data:          # Named volume — persists across stop/down/reboot
  ui-node-modules:   # Prevents host/container node_modules conflicts
```

### 12.6 API `Dockerfile` (Multi-Stage, Hot Reload)

```dockerfile
# docker/api/Dockerfile
# Target "dev" for local development with hot reload (docker compose watch / volume mount).
# Target "prod" for the publishable image (used by CI/CD — see Section 8).

# ── .NET 10 SDK base ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS sdk
WORKDIR /app

# ── Development (hot reload) ──────────────────────────────────
FROM sdk AS dev
# dotnet watch polls for file changes signaled by the volume mount.
ENTRYPOINT ["dotnet", "watch", "run", "--project", "src/Api/Api.csproj", "--no-hot-reload-profile"]
# NOTE: If watch fails to detect changes, try `docker compose restart api`.

# ── Production publish ────────────────────────────────────────
FROM sdk AS build
COPY . .
RUN dotnet restore Synergistic.sln
RUN dotnet publish src/Api/Api.csproj -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS prod
WORKDIR /app
COPY --from=build /publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Api.dll"]
```

### 12.7 UI `Dockerfile` (Multi-Stage, Hot Reload)

```dockerfile
# docker/ui/Dockerfile
# Target "dev" for local development with Angular dev server and HMR.
# Target "prod" for the static-build image served by nginx.

# ── Node base ─────────────────────────────────────────────────
FROM node:22-alpine AS node
WORKDIR /app

# ── Development (hot reload) ──────────────────────────────────
FROM node AS dev
COPY package*.json ./
RUN npm ci
COPY . .
# ng serve with host 0.0.0.0 so the container port is reachable from the host.
# Polling enabled for cross-OS volume mount compatibility (Windows host → Linux container).
CMD ["npx", "ng", "serve", "--host", "0.0.0.0", "--port", "4200", "--poll", "2000"]
# NOTE: --poll ensures file-change detection works reliably with host volumes.

# ── Production build ──────────────────────────────────────────
FROM node AS build
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine AS prod
COPY --from=build /app/dist/icm-admin/browser /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### 12.8 `.dockerignore`

```dockerignore
# .dockerignore — reduce build context sent to Docker daemon

# Dependencies
**/node_modules/
**/bin/
**/obj/

# Build outputs
**/dist/
**/out/

# IDE & OS
**/.vs/
**/.vscode/
**/.idea/
*.user
*.suo
.DS_Store
Thumbs.db

# Git
.git/
.gitignore
.gitattributes

# Environment
.env
*.env.local

# Documentation & feature workspace
docs/
feature-workspace/

# Logs & temp
*.log
*.tmp
```

### 12.9 `.env` (Secrets & Config)

```ini
# .env — local Docker Compose configuration
# ⚠ NEVER commit this file. It is excluded by .dockerignore.

SA_PASSWORD=DevPassword123!
```

Create `.env.example` for the team:

```ini
# .env.example — committed template. Copy to .env and set your own password.
SA_PASSWORD=ChangeMe123!
```

### 12.10 Database Migration Runner

The `entrypoint.sh` script runs after SQL Server is healthy, applying all `.sql` migration scripts in order:

```bash
#!/bin/bash
# docker/db/entrypoint.sh
# Runs after SQL Server starts. Applies migration scripts from /migrations in
# alphabetical order, tracking each one in dbo.SchemaVersion (created by 001_).

set -e

echo "Waiting for SQL Server to be ready..."
for i in $(seq 1 30); do
  if /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "${MSSQL_SA_PASSWORD}" -Q "SELECT 1" &>/dev/null; then
    echo "SQL Server is ready."
    break
  fi
  echo "  ...waiting ($i/30)"
  sleep 2
done

echo "Running migrations..."
for f in /migrations/*.sql; do
  script_name=$(basename "$f")
  echo "  Applying: $script_name"
  /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "${MSSQL_SA_PASSWORD}" -d master -i "$f"
done

echo "All migrations applied."
```

### 12.11 Daily Workflow

```powershell
# First time (or after cloning)
docker compose up -d           # Build images, create volumes, start all services
                               # SQL migrations run automatically on first start

# Day-to-day development
docker compose start            # Resume stopped containers — all data intact

# After pulling new migrations or dependency changes
docker compose up -d --build    # Rebuild images and restart

# Stop for the day
docker compose stop             # Containers stop, volumes/data preserved

# Full teardown (keep data)
docker compose down             # Removes containers & networks; VOLUMES SURVIVE

# Complete reset (WARNING: deletes ALL data)
docker compose down -v          # Removes containers, networks, AND volumes
```

### 12.12 Verifying the Stack

| Service | URL / Connection | What to Check |
|---|---|---|
| Angular SPA | http://localhost:4200 | App loads, no console errors |
| .NET API | http://localhost:5001/api/health | Returns 200 OK |
| SQL Server (SSMS) | `localhost,1433` — `sa` / password from `.env` | `icm-dev` database exists, `dbo.SchemaVersion` table populated |

##### Health check endpoint

The `HealthController` is already wired at `GET /api/health` and returns a simple status response. Verify with:

```powershell
Invoke-RestMethod http://localhost:5001/api/health
```

### 12.13 Troubleshooting

| Symptom | Likely Cause | Fix |
|---|---|---|
| `docker compose up` fails with port conflict | Port 1433, 4200, or 5001 already in use | Stop local SQL Server / IIS / another Angular dev server, or change the host port in `docker-compose.yml` |
| Angular HMR doesn't detect file changes | Inotify not working across host→container volume on Windows | The `--poll 2000` flag is already set in the Dockerfile. If still broken, increase to `--poll 1000` |
| SQL migrations don't run | SQL Server wasn't healthy when entrypoint ran | `docker compose restart db` — the healthcheck will pass and entrypoint will re-run |
| "Login failed for user 'sa'" in SSMS | Password mismatch or SQL not ready | Check `.env` for `SA_PASSWORD`. Wait for `icm-db` container to show `healthy` in `docker compose ps` |
| API can't reach SQL | Container not on same network or DNS resolution failed | Ensure `depends_on: db (healthy)` is in the compose file. Use `db` (the service name) as hostname |

---

## 13. Document Governance

| Attribute | Detail |
|---|---|
| Owner | Platform Architecture Team |
| Version | 1.0 |
| Last Updated | 2026-07-19 |
| Review Cadence | Quarterly (or after major architectural decision) |
| Classification | Internal — Engineering |