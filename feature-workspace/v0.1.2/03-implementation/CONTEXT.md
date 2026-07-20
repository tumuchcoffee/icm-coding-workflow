# Implementation Specifications

## Stage Purpose

Transform the architecture blueprint from [Stage 02](../02-architecture/output/) into working, production-ready code. This stage produces the concrete implementation artifacts — source files, SQL scripts, configuration, and tests — that realize the architecture. Every line of code written here must be traceable to an architecture decision and must conform to the project's coding standards.

The single consolidated deliverable summarizing all implementation work is saved to `.\output\implementation-results.md`. That file is the primary input for Stage 04 (Review & Testing).

---

## Inputs

| Layer                            | Source                                | What to Load                                                                                              | Why                                             |
| -------------------------------- | ------------------------------------- | --------------------------------------------------------------------------------------------------------- | ----------------------------------------------- |
| **Layer 3 (Reference)**    | `../../docs/coding-standards.md`    | Language conventions for C#, Angular/TypeScript, SQL Server, and Azure                                    | Enforce consistency across all generated code   |
| **Layer 3 (Reference)**    | `../../docs/system-architecture.md` | System-level component boundaries, tech stack, solution structure                                         | Place code in the correct project and layer     |
| **Layer 4 (Architecture)** | `../02-architecture/output/*.md`    | All architecture artifacts (ADRs, component design, data model, sequence diagrams, component interaction) | Defines the exact blueprint to implement        |
| **Layer 4 (Working)**      | `./references/*.md`                 | Any implementation-specific reference material (code samples, third-party docs, migration notes)          | Supplement architecture with practical guidance |

> **Important**: Read every file matching `../02-architecture/output/*.md`. Do not skip any — if a file exists in that directory, it is required input. If the architecture output directory is empty or missing expected files, stop and report the gap; do not guess.

---

## Process

### 1. Ingest All Inputs

1. **Load coding standards.** Read `../../docs/coding-standards.md` in full. Make a checklist of every convention that will apply to the languages you will generate (C#, TypeScript, SQL, Azure IaC).
2. **Load system architecture.** Read `../../docs/system-architecture.md`. Note the solution structure, layer boundaries, technology versions, and naming conventions.
3. **Load all architecture outputs.** Scan `../02-architecture/output/` and load every `.md` file. Identify:
   - **ADR decisions** — every architectural choice that constrains implementation.
   - **Component specifications** — every class, endpoint, component, and function to build.
   - **Data model changes** — every table, column, index, and migration to create.
   - **Sequence diagrams** — every flow to implement (happy path + error paths).
   - **Component interaction diagram** — how the new pieces connect to the existing system.
4. **Load reference materials.** Scan `./references/` and load every `.md` file. Note any additional constraints, examples, or third-party integration details.

### 2. Plan Implementation Order

Determine the dependency-ordered build sequence. The order should be:

1. **Domain layer** — entities, value objects, domain events, and exceptions (zero dependencies).
2. **Application layer** — commands, queries, handlers, validators, DTOs, and interfaces (depends on Domain).
3. **Infrastructure layer** — repository implementations, external service clients, messaging consumers (depends on Application interfaces).
4. **API layer** — controller definitions, middleware, DI registration (depends on Application and Infrastructure).
5. **Database** — SQL migration scripts, stored procedures, indexes (can be written in parallel with layers 1–4).
6. **Frontend** — Angular components, services, stores, and templates (depends on API contracts).
7. **Tests** — unit tests for each layer, integration tests for critical flows (written alongside each layer, not after).

Document the build order in the implementation results.

### 3. Implement Each Layer

For each layer, follow this cycle:

1. **Read the architecture spec** for the component you're about to build.
2. **Write the code** following the coding standards exactly. Cite the standard you're applying in code comments where the rationale isn't obvious.
3. **Write the tests** for that component before moving on. Every handler, validator, service, and controller action must have corresponding tests.
4. **Validate** that the code:
   - Respects Clean Architecture layer boundaries (dependencies point inward).
   - Handles errors explicitly — no "happy path only" code.
   - Is tenant-aware (includes `TenantId` in multi-tenant paths).
   - Produces structured logs with `CorrelationId`, `TenantId`, and `UserId`.
   - Is idempotent where required (SQL scripts, PUT/DELETE controller actions).

#### Layer-Specific Guidance

##### Domain Layer

- Entities must have parameterized constructors that enforce invariants.
- Value objects must be immutable and implement structural equality.
- Domain events must be immutable and carry only the data needed by handlers.
- No references to Infrastructure, Application, or Api projects.

##### Application Layer

- Commands and queries are `IRequest<T>` implementations routed through MediatR.
- Use FluentValidation validators for every command and query.
- DTOs must never expose domain entities directly — always map.
- Interfaces defined here (`IRepository`, `IService`) must be small and role-focused (Interface Segregation Principle).

##### Infrastructure Layer

- Dapper SQL must be parameterized — never concatenate input into SQL strings.
- Repository implementations must use `IDbConnection` from the DI container.
- External HTTP calls must use Polly resilience policies (retry, circuit breaker, timeout).
- Secrets and connection strings come from `IConfiguration` / Key Vault — never hardcoded.

##### API Layer

- Use ASP.NET Core Controllers inheriting from `ControllerBase` with `[ApiController]` and `[Route]` attributes.
- Every controller action must include `[Authorize]` unless explicitly public.
- Return Problem Details (RFC 7807) for all error responses.
- Controllers are a thin shell — map HTTP to MediatR commands/queries and return results.
- Include `x-api-version` header support or URL path versioning.
- Organize controllers by feature (`Controllers/HealthController.cs`, `Controllers/TenantsController.cs`).

##### Database (SQL)

- Every script must be idempotent: check for object existence before `CREATE` or `ALTER`.
- Always specify the schema (`dbo.`) when referencing objects.
- Include `TenantId` on every table in multi-tenant scope.
- Use GUIDs for primary keys exposed to clients; integers for internal-only tables are acceptable.
- No cascading deletes — handle referential integrity in application logic.
- Index foreign key columns and common query predicates.
- Include meaningful comments explaining intent in complex queries and stored procedures.

##### Frontend (Angular)

- Use standalone components only — no `NgModule`.
- Use Signals (`signal()`, `computed()`, `effect()`) for local state; NgRx SignalStore for feature state.
- Use new control flow syntax (`@if`, `@for`).
- Services encapsulate all HTTP calls; components never call `HttpClient` directly.
- Use `async` pipe for observable subscriptions in templates.
- Include `X-Tenant-Id` and `X-Correlation-Id` headers via HTTP interceptors.
- Organize files by feature, not by type.

##### Tests

- **Unit tests**: xUnit + NSubstitute for .NET; Jest for Angular. Every handler, validator, service, and controller action must have unit tests.
- **Integration tests**: Testcontainers for database integration; `WebApplicationFactory` for API integration tests.
- **E2E tests**: Playwright for critical user journeys.
- Test naming convention: `MethodName_Scenario_ExpectedBehavior`.
- Follow AAA pattern (Arrange, Act, Assert).
- Tests must be independent — no shared state between tests.

### 4. Write the Implementation Results

Save to `.\output\implementation-results.md` using the following structure, which is designed to be consumed by Stage 04 (Review & Testing):

```markdown
# Implementation Results

**Feature**: [Feature Name]
**Date**: YYYY-MM-DD
**Architecture Reference**: ../02-architecture/output/

---

## 1. Implementation Summary

- **Layers implemented**: [Domain, Application, Infrastructure, Api, Database, Frontend, Tests]
- **Total files created**: ##
- **Total files modified**: ##
- **Build order followed**: [numbered sequence]

---

## 2. File Manifest

A complete list of every file created or modified, organized by layer and project.

| # | File Path | Operation | Language | Purpose | Architecture Trace |
|---|-----------|-----------|----------|---------|-------------------|
| 1 | `src/02-backend/Domain/Entities/...` | Created | C# | ... | ADR-001, Component: ... |
| ... | ... | ... | ... | ... | ... |

---

## 3. Architecture Traceability Matrix

Every architecture decision and component design must map to at least one implementation artifact.

| Architecture Artifact | Decision / Component | Implemented In (file paths) | Status |
|-----------------------|---------------------|----------------------------|--------|
| ADR-001 | ... | ... | ✅ Implemented |
| Component: ... | ... | ... | ✅ Implemented |

---

## 4. Coding Standards Compliance

| Standard | Source | Applied In (files/layers) | Status |
|----------|--------|--------------------------|--------|
| C# PascalCase | coding-standards.md#C# | Domain/, Application/, etc. | ✅ |
| ... | ... | ... | ... |

---

## 5. Test Coverage

| Layer | Unit Tests | Integration Tests | E2E Tests | Notes |
|-------|-----------|-------------------|-----------|-------|
| Domain | ## | — | — | |
| Application | ## | — | — | |
| Infrastructure | — | ## | — | |
| Api | — | ## | — | |
| Database | — | ## | — | |
| Frontend | ## | — | ## | |

---

## 6. Deviations & Rationale

Any place where the implementation intentionally diverged from the architecture, with justification.

| Deviation | Architecture Reference | Reason | Risk / Mitigation |
|-----------|----------------------|--------|-------------------|
| ... | ... | ... | ... |

---

## 7. Known Issues & Open Items

- [ ] Issue or limitation (severity, impact, recommended resolution)
- [ ] Item deferred to a follow-up phase

---

## 8. Stage 04 Handoff Notes

Specific notes for the reviewer/tester:
- Which files to review first (highest risk / most critical path).
- Which test commands to run and expected results.
- Any environment prerequisites (connection strings, emulators, certificates).
- Smoke test steps: the minimum actions to verify the feature works end-to-end.
```

---

## Outputs

| File                                 | Purpose                                                                                                                            |
| ------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------- |
| `output/implementation-results.md` | Consolidated implementation summary, manifest, traceability matrix, and Stage 04 handoff — the primary deliverable for this stage |
| Source files (in`../../source/`)   | All created or modified code, SQL, IaC, and configuration files                                                                    |

> The implementation results file is the **single required output** of this stage. The source files are the actual implementation artifacts written to the appropriate `../../source/` directories. Stage 04 will use the results file to know exactly what to review and test.

---

## Quality Checklist

Before marking this stage complete, verify:

- [ ] Every architecture decision (ADR) has at least one corresponding implementation artifact traceable in the results manifest
- [ ] Every component specified in `component-design.md` has been implemented
- [ ] Every data model change from `data-model.md` has been applied (SQL scripts exist and are idempotent)
- [ ] Every sequence diagram flow (happy path + error paths) is exercised by at least one test
- [ ] Clean Architecture layer dependencies point inward — no leaks
- [ ] All SQL is parameterized — no string concatenation of user input
- [ ] Multi-tenancy is enforced: `TenantId` on every relevant table, header on every request
- [ ] Error handling is explicit on every path — no unhandled exceptions expected in normal operation
- [ ] All external calls (HTTP, Service Bus, database) have retry/timeout policies
- [ ] Secrets are never in code or config files
- [ ] Observability: structured logs include `CorrelationId`, `TenantId`, and `UserId`; metrics exist for key operations
- [ ] Coding standards from `../../docs/coding-standards.md` are followed for every language used
- [ ] Tests exist for every handler, validator, service, controller action, and Angular component
- [ ] All tests pass and are independent (no shared state, no ordering assumptions)
- [ ] The implementation results file (`output/implementation-results.md`) is complete and self-contained — a reviewer can understand everything without opening individual source files
- [ ] The results file includes clear smoke test steps for Stage 04
