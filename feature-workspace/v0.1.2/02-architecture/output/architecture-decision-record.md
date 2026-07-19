# Architecture Decision Records — Code Initialization (v0.1.2)

**Feature**: Code Initialization — project scaffolding, containers, and database setup
**Date**: 2026-07-18
**Version**: v0.1.2

---

## ADR-001: Simplified Project Structure — No MediatR / Dapper / CQRS in v0.1.2

**Status**: Accepted
**Date**: 2026-07-18
**Context**:
The system architecture specifies a full Clean Architecture stack with MediatR, Dapper, FluentValidation, and CQRS. However, v0.1.2 has zero business logic — the only backend requirement is a single health check endpoint (FR-007). Introducing MediatR, CQRS pipelines, and Dapper repositories at this stage would add scaffolding with no immediate value and create busywork for future refactoring when real features arrive.

**Decision**:
Use a **graduated complexity** approach. The v0.1.2 backend will follow Clean Architecture's layering (Api → Application → Domain → Infrastructure) but will omit MediatR, Dapper, FluentValidation, and CQRS. The health check endpoint will be a single Minimal API route in `Api/Endpoints/Health/`. The `Application` and `Domain` layers will be created as empty placeholder projects with their correct dependency structure, ready for real features in future versions. The `Infrastructure` layer will be created as a placeholder — it has no implementations yet (no database access in v0.1.2).

**Alternatives Considered**:
1. **Full Clean Architecture with MediatR from day one** — Rejected: premature abstraction. A health check endpoint does not benefit from MediatR pipelines, validation behaviors, or repository patterns. Adding them now creates ceremony without value.
2. **Single-project "everything in Api"** — Rejected: violates the layered architecture principles in the system architecture doc. Future features would need to break apart an already-entangled codebase.
3. **Skip Application/Domain layers entirely** — Rejected: same as above. Establishing the layer boundaries now, even if thin, makes future work easier.

**Consequences**:
- **Easier**: Reduced boilerplate for v0.1.2. Developers can see the health endpoint immediately without tracing through MediatR pipelines.
- **Harder**: When FR-007 evolves (e.g., a database-connected health check in a future version), the team will need to introduce MediatR and Dapper at that point — but that is the right time to do it.
- **Follow-up**: v0.2.0 or the first feature that accesses the database must introduce MediatR, Dapper, and FluentValidation as per the system architecture.
- **Risk**: Low. The layer structure is in place; only the MediatR/Dapper wiring is deferred.

**Principles Applied**:
- **YAGNI** (You Aren't Gonna Need It) — don't build infrastructure before the feature demands it
- **Single Responsibility** — each layer has a clear purpose, even if thin
- **Open/Closed** — the layer structure is open for extension (adding MediatR later) without modifying the Api layer's responsibility

---

## ADR-002: Minimal API Endpoints over Controller-Based APIs

**Status**: Accepted
**Date**: 2026-07-18
**Context**:
The requirements doc references a "Health Check controller" (requirements.md), but the system architecture (`docs/system-architecture.md`, Section 4.5) explicitly specifies Minimal API endpoint groups as the pattern for the .NET 10 backend. FR-004 of the requirements gap analysis flagged this contradiction.

**Decision**:
Follow the system architecture: use **Minimal API endpoint groups**. The health check will be defined as a route group in `Api/Endpoints/Health/HealthEndpoints.cs` using `MapGet`. The source requirements' use of the word "controller" is treated as a colloquialism, not a technical mandate.

**Alternatives Considered**:
1. **Controller-based API** — Rejected: contradicts the established system architecture. The system architecture is the authority for cross-cutting technical decisions. Switching back to controllers for one features creates inconsistency.
2. **Hybrid approach** — Rejected: mixing controllers and Minimal APIs in the same project is confusing and violates consistency.

**Consequences**:
- **Easier**: Consistency with the rest of the codebase. Lower ceremony for simple endpoints. Fewer files per endpoint.
- **Harder**: Developers used to `[ApiController]` conventions must learn the Minimal API pattern. The learning curve is shallow — one `MapGet` line for the health endpoint.
- **Risk**: Low. The system architecture already adopts Minimal APIs organization-wide.

**Principles Applied**:
- **Separation of Concerns** — Endpoint mapping is separated from business logic by layer
- **Consistency** — Follow the established system-level patterns
- **Simple over Complex** — Minimal APIs are the simpler choice for this feature

---

## ADR-003: Standalone Components with PrimeNG for UI Shell

**Status**: Accepted
**Date**: 2026-07-18
**Context**:
FR-011 mandates PrimeNG as the default UI component library. FR-012 mandates Angular latest LTS with standalone components and new control flow syntax (`@if`, `@for`). The system architecture (Section 3.3) confirms standalone components as the project pattern.

**Decision**:
All Angular components (Header, Menu, Footer, DetailPane, AppShell) will be **standalone components** with no NgModules. The UI shell will use **PrimeNG** components:
- `p-button` for the hamburger icon
- `p-avatar` for the user profile icon
- `p-sidebar` for the slide-out navigation menu
- PrimeNG's layout primitives as needed

PrimeNG's utility CSS classes and built-in layout primitives will handle positioning, sizing, and spacing. No custom CSS framework.

**Alternatives Considered**:
1. **Angular Material** — Rejected: FR-011 explicitly requires PrimeNG.
2. **Custom CSS only (no component library)** — Rejected: contradicts FR-011. A component library accelerates development and provides accessibility baseline.
3. **NgModule-based architecture** — Rejected: contradicts FR-012 and the system architecture.

**Consequences**:
- **Easier**: Rapid UI shell construction with pre-built accessible components. Consistent look and feel.
- **Harder**: PrimeNG's theming system (SASS variables, CSS custom properties) must be learned. Styling customization happens through PrimeNG's theme configuration rather than utility classes.
- **Follow-up**: PrimeNG theme and styling strategy must be documented in the component design.
- **Risk**: Low. PrimeNG provides a comprehensive theming API; components can be styled consistently through its design tokens.

**Principles Applied**:
- **Single Responsibility** — each UI component does one layout job
- **Dependency Inversion** — the shell components depend on PrimeNG abstractions, not custom implementations of common controls
- **Separation of Concerns** — UI structure (Angular) vs. UI styling (PrimeNG theme) vs. UI controls (PrimeNG components)

---

## ADR-008: Separate Template and Style Files per Component

**Status**: Accepted
**Date**: 2026-07-18
**Context**:
The initial component design specifications for v0.1.2 used inline `template` strings in the TypeScript files, with styling embedded in the same file (either inline or via PrimeNG utility CSS classes). While inline templates are convenient during rapid prototyping, they degrade maintainability as components grow. A component that mixes TypeScript logic, HTML structure, and SCSS styling in a single file violates the Single Responsibility Principle at the file level.

**Decision**:
Every Angular component **must** use separate files for its TypeScript, HTML template, and SCSS styles:

- `*.component.ts` — TypeScript logic only (class definition, inputs, outputs, DI, signals)
- `*.component.html` — HTML template (structural markup, Angular control flow, component/directive references)
- `*.component.scss` — SCSS styles (component-scoped styles, no global styles)

The TypeScript file references the other two via `templateUrl` and `styleUrls`:

```typescript
@Component({
  selector: 'app-header',
  standalone: true,
  imports: [Button, Avatar, RouterLink],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent {
  menuToggle = output<void>();
}
```

This applies to all future Angular components in the project — not just v0.1.2 shell components. Global styles (theme overrides, CSS custom properties) remain in the root `styles.scss`.

**Alternatives Considered**:
1. **Inline templates and styles (current approach)** — Rejected for maintainability reasons. A 30-line template plus 20 lines of styles plus 30 lines of TypeScript in a single file is harder to review, harder to diff, and encourages developers to add logic to the template that belongs in the component class.
2. **Template file only, inline styles** — Rejected: half measures. If we're separating structure from logic, separating presentation as well is the natural next step. SCSS files benefit from syntax highlighting, linting, and IDE autocomplete that inline styles do not get.
3. **CSS (not SCSS) files** — Rejected: the Angular CLI's default style format is SCSS. Using plain CSS would lose access to variables, nesting, mixins, and the SCSS features PrimeNG's theming system relies on. The system architecture already assumes SCSS via PrimeNG's SASS theming design tokens.

**Consequences**:
- **Easier**: Code reviews are clearer — HTML changes, TS logic changes, and styling changes appear in separate diffs. SCSS files get full IDE support (syntax highlighting, autocomplete, linting). New developers can understand a component by reading the template first, then the class, then the styles.
- **Harder**: Slightly more files per component (3 instead of 1). Component scaffolding must generate three files instead of one. Minor overhead during rapid prototyping. The Angular CLI's `ng generate component` already produces three files by default, so this is the standard Angular workflow, not an added burden.
- **Follow-up**: Update the `component-design.md` output to reflect `templateUrl`/`styleUrls` instead of inline templates. Update solution structure diagrams to show `.html` and `.scss` files.
- **Risk**: Very low. This is the default Angular CLI behavior and is well-supported by every Angular tool.

**Principles Applied**:
- **Single Responsibility** — each file has exactly one reason to change (logic, structure, or presentation)
- **Separation of Concerns** — HTML, SCSS, and TypeScript are distinct languages with distinct concerns; they should be in distinct files
- **Consistency** — aligns with the Angular Style Guide recommendation to use `templateUrl` and `styleUrls` for components with more than a few lines of template or styles
- **Open/Closed** — separating files makes it easier to extend styling without touching logic, and vice versa

---

## ADR-004: SQL Server LocalDB with Versioned Migration Scripts

**Status**: Accepted
**Date**: 2026-07-18
**Context**:
FR-008 requires a SQL Server database named `Synergistic` with a `dbo.SchemaVersion` table for tracking migrations. NFR-005 requires the entire stack to run on a single developer machine with no cloud dependencies.

**Decision**:
Use **SQL Server LocalDB** (which ships with Visual Studio and the SQL Server Express LocalDB installer) as the local development database engine. Use **DbUp** for versioned, idempotent migration scripts. The migration scripts will be stored in `source/03-sql/migrations/` and a scripted copy of all database objects will be generated into `source/03-sql/script/` for AI model access.

The first migration (`001_CreateSchemaVersion.sql`) will:
1. Create the `dbo.SchemaVersion` table if it does not exist
2. Insert a row marking migration `001` as applied

The startup script (`run.ps1`) will run DbUp to apply any pending migrations before launching the API.

**Alternatives Considered**:
1. **Docker-based SQL Server** — Rejected: adds Docker as a prerequisite. NFR-005 asks for a lightweight local setup. LocalDB is simpler.
2. **SQLite** — Rejected: the system architecture specifies SQL Server. Using a different engine locally creates drift between dev and prod.
3. **EF Core Migrations** — Rejected: the system architecture specifies Dapper, not EF Core. DbUp is Dapper's natural companion for schema management.
4. **Manual SQL scripts without migration tooling** — Rejected: no version tracking, no idempotency guarantees, error-prone.

**Consequences**:
- **Easier**: Lightweight local setup. No Docker required. Same engine family as production (SQL Server).
- **Harder**: LocalDB has limitations (no remote connections, single instance). Not suitable for shared dev environments.
- **Follow-up**: When the project moves to Azure, DbUp will run the same migrations against Azure SQL Database.
- **Risk**: Low. LocalDB is the standard local development option for SQL Server.

**Principles Applied**:
- **Infrastructure as Code** — database schema is code, versioned and repeatable
- **Idempotency** — all migration scripts check for existence before creating
- **Separation of Concerns** — schema management (DbUp) is separate from data access (Dapper, when it's introduced)

---

## ADR-005: No Multi-Tenancy Implementation in v0.1.2

**Status**: Accepted
**Date**: 2026-07-18
**Context**:
The system architecture mandates multi-tenancy at every layer — `TenantId` on every table, tenant resolution middleware, and tenant-scoped queries. However, FR-009 explicitly states no authentication in v0.1.2, and there are no tenant-related functional requirements in this version. The backlog marks multi-tenant middleware as "Won't Have (This Time)."

**Decision**:
**Do not implement tenant infrastructure in v0.1.2.** The `dbo.SchemaVersion` table will not include a `TenantId` column — it's an infrastructure table, not a tenant-scoped entity. When tenant support is added in a future version (after authentication), the `TenantId` column will be added to all new entity tables at that time, not retrofitted to the `SchemaVersion` table.

**Alternatives Considered**:
1. **Add `TenantId` to `SchemaVersion` now** — Rejected: the table tracks global migration state, not tenant data. Adding `TenantId` would complicate migration logic with no benefit.
2. **Pre-emptively build tenant middleware** — Rejected: no authentication exists to resolve a tenant. The middleware would have nothing to extract.

**Consequences**:
- **Easier**: Simpler database schema. No tenant plumbing in the API.
- **Harder**: When multi-tenancy is added later, it will be a new concern — but that's appropriate.
- **Risk**: Very low. `SchemaVersion` is a system table that should never be tenant-scoped.

**Principles Applied**:
- **YAGNI** — don't build tenant infrastructure before it's needed
- **Single Responsibility** — `SchemaVersion` tracks migrations, not tenant data
- **Incremental Complexity** — add concerns when they have a functional requirement driving them

---

## ADR-006: No Observability Infrastructure in v0.1.2

**Status**: Accepted
**Date**: 2026-07-18
**Context**:
The system architecture dedicates an entire section (Section 9) to observability — Serilog → Cosmos DB, Application Insights, distributed tracing, and alerting. However, v0.1.2 is local-dev-only (NFR-005) with zero Azure dependencies. There are no Cosmos DB instances, no Application Insights resources, and no Service Bus to trace across. The NFR checklist confirms observability is "Deferred to future versions."

**Decision**:
**Use console logging only.** The .NET API will use the default `Microsoft.Extensions.Logging` console provider. Serilog and Cosmos DB sinks will be introduced when the project deploys to Azure (future version). Correlation IDs and structured logging fields will not be implemented in v0.1.2.

**Alternatives Considered**:
1. **Serilog with file sink locally** — Rejected: adds a NuGet dependency for no operational benefit over console output in dev.
2. **Serilog with Seq locally** — Rejected: adds another container/service to the local stack. Violates the "single command" ethos of FR-001.
3. **Full Application Insights SDK** — Rejected: requires an Azure subscription and an Application Insights resource. Contradicts NFR-005.

**Consequences**:
- **Easier**: No telemetry configuration. `Console.WriteLine`-level output from the default logger.
- **Harder**: When observability is added later, Serilog must be configured and the logging pipeline must be restructured.
- **Follow-up**: The first Azure-deployed version must add Serilog + Cosmos DB as specified in the system architecture.
- **Risk**: Low. This is a local-only development foundation.

**Principles Applied**:
- **YAGNI** — don't provision observability infrastructure before there's anything to observe
- **Simple over Complex** — console logging is sufficient for a single health endpoint on localhost
- **Incremental Complexity** — add concerns when the deployment model demands them

---

## ADR-007: Postman Collection for API Verification

**Status**: Accepted
**Date**: 2026-07-18
**Context**:
FR-010 requires a Postman collection in `\source\04-testing\postman` with a health check request that validates HTTP 200, `status`, `timestamp`, and `version` fields. This is the only testing artifact required for v0.1.2 — no unit tests, no E2E tests, no integration tests are in scope.

**Decision**:
Create a **single Postman collection file** (`icm-admin-v0.1.2.postman_collection.json`) with:
- A `baseUrl` collection variable defaulting to `http://localhost:5001`
- One GET request to `{{baseUrl}}/api/health`
- Test scripts verifying HTTP 200 and the presence of `status`, `timestamp`, and `version` fields in the JSON response

The collection will be structured with folders for future extension but will contain only the health check request in v0.1.2.

**Alternatives Considered**:
1. **xUnit integration test** — Rejected: the requirements explicitly ask for a Postman collection, not a C# test.
2. **Playwright E2E test hitting the API** — Rejected: overkill for a single GET endpoint. Postman is simpler and more accessible.
3. **Manual curl documentation** — Rejected: less repeatable than a Postman collection with automated assertions.

**Consequences**:
- **Easier**: Quick API verification. Import → Run → See green checkmarks.
- **Harder**: Postman is a manual tool unless run via Newman in CI. For v0.1.2 this is acceptable.
- **Risk**: None.

**Principles Applied**:
- **Separation of Concerns** — testing artifacts live in the testing folder, not mixed with source code
- **Repeatability** — automated assertions ensure consistent verification across developers

---

## Decision Summary

| ADR | Topic | Status |
|-----|-------|--------|
| ADR-001 | No MediatR/Dapper/CQRS yet — graduated complexity | Accepted |
| ADR-002 | Minimal API endpoints over controllers | Accepted |
| ADR-003 | Standalone components + PrimeNG | Accepted |
| ADR-004 | SQL Server LocalDB + DbUp migrations | Accepted |
| ADR-005 | No multi-tenancy in v0.1.2 | Accepted |
| ADR-006 | Console logging only — no observability infra | Accepted |
| ADR-007 | Postman collection for API verification | Accepted |