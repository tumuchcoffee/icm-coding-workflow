# Component Interaction Diagram — Code Initialization (v0.1.2)

**Feature**: Code Initialization — project scaffolding, containers, and database setup
**Date**: 2026-07-18
**Version**: v0.1.2

---

## 1. System-Level Integration

This diagram shows how the v0.1.2 components map onto the established system architecture from `docs/system-architecture.md`. Components prefixed with `(v0.1.2)` are new or modified in this version. Components in gray are part of the system architecture but **not implemented** in v0.1.2.

```mermaid
graph TD
    subgraph "Client Layer"
        A["Angular SPA<br/>localhost:4200<br/>(v0.1.2: App Shell)"]
    end

    subgraph "API Layer"
        B[".NET 10 Web API<br/>localhost:5001<br/>(v0.1.2: Health Endpoint)"]
    end

    subgraph "Data Layer"
        E["SQL Server LocalDB<br/>Synergistic<br/>(v0.1.2: SchemaVersion)"]
    end

    subgraph "Testing"
        PM["Postman Collection<br/>(v0.1.2: Health Check Test)"]
    end

    subgraph "Startup"
        PS["run.ps1<br/>(v0.1.2: Orchestration Script)"]
    end

    subgraph "Identity & Security — Deferred"
        style H fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        style I fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        H["Azure AD B2C / Entra ID<br/>Auth & MFA"]
        I["Azure Key Vault<br/>Secrets & Certificates"]
    end

    subgraph "Integration & Events — Deferred"
        style C fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        style D fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        C["Azure Service Bus<br/>Topics & Queues"]
        D["Azure Functions<br/>Background Workers"]
    end

    subgraph "Azure Infrastructure — Deferred"
        style FD fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        style F fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        style G fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        style J fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        style K fill:#f0f0f0,stroke:#ccc,stroke-dasharray: 5 5
        FD["Azure Front Door<br/>Global LB + WAF"]
        F["Azure Blob Storage"]
        G["Azure Cache for Redis"]
        J["Application Insights"]
        K["Azure Cosmos DB<br/>Log Store"]
    end

    A -->|"GET /api/health (HTTP)"| B
    B -->|"DbUp Migrations (startup)"| E
    PS -->|"Phase 1: Apply migrations"| E
    PS -->|"Phase 2: dotnet run"| B
    PS -->|"Phase 3: npm start"| A
    PM -->|"GET /api/health (HTTP)"| B
```

---

## 2. Angular SPA — Internal Component Structure

Shows how the v0.1.2 Angular components compose into the app shell.

```mermaid
graph TD
    subgraph "Angular SPA — localhost:4200"
        AS["AppShellComponent<br/>(layout grid host)"]

        subgraph "Layout Components"
            H["HeaderComponent<br/>sticky, z-40<br/>(FR-002)"]
            M["MenuComponent<br/>slide-out overlay<br/>(FR-003)"]
            F["FooterComponent<br/>sticky bottom<br/>(FR-004)"]
            DP["DetailPaneComponent<br/>optional, right<br/>(FR-005)"]
        end

        RO["Router Outlet<br/>(FR-006)"]
        R["Angular Router<br/>app.routes.ts"]

        subgraph "External Dependencies"
            PNG["PrimeNG<br/>p-button, p-avatar, p-sidebar"]
        end
    end

    AS --> H
    AS --> M
    AS --> F
    AS --> DP
    AS --> RO
    RO --> R

    H -->|"imports"| PNG
    M -->|"imports"| PNG

    H -->|"menuToggle event"| AS
    AS -->|"isOpen signal"| M
    M -->|"closed event"| AS
    AS -->|"isOpen signal"| DP

    style AS fill:#e3f2fd,stroke:#1565c0
    style H fill:#fff3e0,stroke:#ef6c00
    style M fill:#fff3e0,stroke:#ef6c00
    style F fill:#fff3e0,stroke:#ef6c00
    style DP fill:#fff3e0,stroke:#ef6c00
```

---

## 3. .NET Solution — Layer Dependencies

Shows the Clean Architecture layer structure and which layers contain v0.1.2 code.

```mermaid
graph TD
    subgraph ".NET Solution — Synergistic.sln"
        subgraph "Api Layer"
            API["Api/Program.cs<br/>Api/Endpoints/Health/<br/>HealthEndpoints.cs"]
        end

        subgraph "Application Layer"
            APP["Application/Features/Health/<br/>HealthCheckResponse.cs<br/>Application/DependencyInjection.cs"]
        end

        subgraph "Domain Layer"
            DOM["Domain.csproj<br/>(empty — placeholder)"]
        end

        subgraph "Infrastructure Layer"
            INF["Infrastructure/<br/>DependencyInjection.cs<br/>(empty — placeholder)"]
        end
    end

    subgraph "External"
        DB["SQL Server LocalDB<br/>Synergistic"]
    end

    API -->|"project reference"| APP
    API -->|"project reference"| INF
    APP -->|"project reference"| DOM
    INF -->|"project reference"| APP
    INF -.->|"future: Dapper"| DB

    style API fill:#c8e6c9,stroke:#2e7d32
    style APP fill:#c8e6c9,stroke:#2e7d32
    style DOM fill:#f5f5f5,stroke:#9e9e9e,stroke-dasharray: 5 5
    style INF fill:#f5f5f5,stroke:#9e9e9e,stroke-dasharray: 5 5
```

**Key**: Green = active code in v0.1.2. Gray/dashed = placeholder (project exists, no active implementations).

---

## 4. Data Flow — Health Check Request

Traces a health check request through the v0.1.2 stack from client to response.

```mermaid
graph LR
    subgraph "Client"
        C1["Browser<br/>(localhost:4200)"]
        C2["Postman<br/>Collection"]
    end

    subgraph ".NET API — Minimal API Pipeline"
        direction TB
        E1["MapGet('/api/health')"]
        E2["HealthCheckResponse<br/>record (Application layer)"]
        E3["JSON Serialization<br/>(System.Text.Json)"]
    end

    subgraph "Response"
        R1["HTTP 200<br/>status: Healthy<br/>timestamp: UTC<br/>version: 0.1.2"]
    end

    C1 -->|"GET /api/health"| E1
    C2 -->|"GET /api/health"| E1
    E1 --> E2
    E2 --> E3
    E3 --> R1
    R1 --> C1
    R1 --> C2
```

**Note**: In v0.1.2, the health check has zero dependencies — no database, no cache, no authentication, no MediatR pipeline. It is the simplest possible endpoint.

---

## 5. Startup Orchestration — run.ps1 Flow

Shows the three-phase startup that brings the full stack online.

```mermaid
graph TD
    START["Developer: ./run.ps1"] --> CHECK["Check Prerequisites<br/>dotnet, node, sqllocaldb"]

    CHECK -->|"all present"| PHASE1
    CHECK -->|"missing"| FAIL["❌ Report missing<br/>dependency + install link"]

    PHASE1["Phase 1: Database<br/>DbUp → LocalDB"] --> PHASE1_OK{"Success?"}
    PHASE1_OK -->|"yes"| PHASE2
    PHASE1_OK -->|"no"| FAIL_MIG["❌ Migration error<br/>Show script + message<br/>DB rolled back"]

    PHASE2["Phase 2: API<br/>dotnet run (background)<br/>→ localhost:5001"] --> PHASE2_OK{"Success?"}
    PHASE2_OK -->|"yes"| PHASE3
    PHASE2_OK -->|"no"| FAIL_API["❌ API start error<br/>Show port/exception"]

    PHASE3["Phase 3: Angular<br/>npm start (foreground)<br/>→ localhost:4200"] --> SUCCESS["✅ Full stack running!<br/>Angular: :4200<br/>API: :5001<br/>DB: Synergistic"]

    style START fill:#e8eaf6,stroke:#3949ab
    style SUCCESS fill:#c8e6c9,stroke:#2e7d32
    style FAIL fill:#ffcdd2,stroke:#c62828
    style FAIL_MIG fill:#ffcdd2,stroke:#c62828
    style FAIL_API fill:#ffcdd2,stroke:#c62828
```

---

## 6. Component Dependency Matrix

| Component | Depends On | Dependency Type |
|---|---|---|
| `AppShellComponent` | `HeaderComponent`, `MenuComponent`, `FooterComponent`, `DetailPaneComponent` | Angular template composition |
| `HeaderComponent` | PrimeNG `p-button`, `p-avatar` | npm package |
| `MenuComponent` | PrimeNG `p-sidebar` | npm package |
| `FooterComponent` | None | Pure HTML/CSS |
| `DetailPaneComponent` | None | Pure HTML/CSS with `@if` |
| `HealthEndpoints.cs` | `HealthCheckResponse` (Application) | .NET project reference |
| `Program.cs` | `Application`, `Infrastructure` | .NET project reference (DI) |
| `HealthCheckResponse` | None (plain record) | No dependencies |
| `001_CreateSchemaVersion.sql` | None | Standalone SQL script |
| `run.ps1` | `dotnet`, `node`, `sqllocaldb` | System PATH |
| Postman Collection | .NET API (`localhost:5001`) | HTTP |

---

## 7. What's NOT Connected (Deferred)

These connections from `docs/system-architecture.md` are intentionally absent in v0.1.2:

| Missing Connection | Why Deferred | When It Arrives |
|---|---|---|
| Angular → Azure AD B2C (auth) | No authentication (FR-009) | Future version with user management |
| Angular → Azure Front Door | Local dev only (NFR-005) | Azure deployment |
| API → MediatR pipeline | No business logic (ADR-001) | First feature with database access |
| API → Dapper → SQL (queries) | No entity tables (ADR-001) | First feature with database access |
| API → Redis cache | No query data to cache | First feature with read-heavy queries |
| API → Service Bus | No async/event-driven flows | First feature needing background work |
| API → Serilog → Cosmos DB | Console logging only (ADR-006) | Azure deployment |
| API → Application Insights | Local dev only (ADR-006) | Azure deployment |
| API → Key Vault (secrets) | No secrets needed (NFR-005) | Azure deployment |
| API → Tenant middleware | No multi-tenancy (ADR-005) | After authentication |