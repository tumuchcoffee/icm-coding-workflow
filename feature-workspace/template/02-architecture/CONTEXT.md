# Architecture Specifications

## Stage Purpose

Translate the requirements from [Stage 01](../01-requirements/output/) into a concrete, implementable architecture. This stage produces the blueprint that Stage 03 (Implementation) will follow. Every decision made here must be traceable to a requirement and grounded in an industry-recognized principle.

---

## Inputs

| Layer                         | Source                                                      | What to Load                                                          |
| ----------------------------- | ----------------------------------------------------------- | --------------------------------------------------------------------- |
| **Layer 3 (Reference)** | `docs/coding-standards.md`                                | Language and platform conventions                                     |
| **Layer 3 (Reference)** | `docs/system-architecture.md`                             | System-level architecture decisions, tech stack, component boundaries |
| **Layer 4 (Working)**   | `../01-requirements/output/feature-requirements-final.md` | Feature requirements, user stories, acceptance criteria               |

---

## Process

### 1. Understand the Requirements

Read every requirement produced by Stage 01. Identify:
- **Functional requirements** — what the system must do
- **Non-functional requirements** — performance, security, scalability, maintainability constraints
- **Integration points** — external services, APIs, databases the feature touches
- **Tenant implications** — how does this feature behave in a multi-tenant context?

### 2. Apply Industry-Recognized Architecture Principles

Use the following principles to guide every decision. When you make a choice, cite the principle that drove it.

#### SOLID Principles (Robert C. Martin)

| Principle                            | Application in This Codebase                                                                                                                                                                                                          |
| ------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **S** — Single Responsibility | Each class, endpoint, and component has exactly one reason to change. Map every new class to a single requirement.                                                                                                                    |
| **O** — Open/Closed           | Extend behavior through interfaces and dependency injection — never by modifying existing, tested classes. Use the MediatR pipeline and Angular providers to inject new behavior.                                                    |
| **L** — Liskov Substitution   | Derived types must be substitutable for their base types. If a subclass can't honor the base contract, rethink the hierarchy.                                                                                                         |
| **I** — Interface Segregation | Keep interfaces small and focused. No class should depend on methods it doesn't use. Prefer role interfaces over fat ones.                                                                                                            |
| **D** — Dependency Inversion  | Depend on abstractions, not concretions. High-level policy (Application layer) never depends on low-level details (Infrastructure). This is enforced by the solution structure —`Application` never references `Infrastructure`. |

#### Clean Architecture / Hexagonal Architecture

```
Frameworks & Drivers  →  Interface Adapters  →  Application Core  →  Domain
(Api, Infrastructure)     (Controllers,        (Use Cases,         (Entities,
                           Presenters)          DTOs, Interfaces)   Value Objects)
```

Rules:
- **Dependencies point inward.** The Domain layer has zero external dependencies.
- **Inner layers define interfaces; outer layers implement them.** E.g., `IUserRepository` lives in Application; the Dapper implementation lives in Infrastructure.
- **The Api layer is a thin shell.** It maps HTTP concerns to MediatR commands/queries and nothing more.

#### Domain-Driven Design (DDD) Tactical Patterns (Eric Evans)

Apply these patterns when the feature involves complex business logic:

| Pattern                  | When to Use                                                                                                                                  |
| ------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------- |
| **Entity**         | An object with a distinct identity that changes over time (e.g.,`Tenant`, `User`, `Order`)                                             |
| **Value Object**   | An immutable object defined by its attributes, not identity (e.g.,`EmailAddress`, `Money`, `Address`)                                  |
| **Aggregate**      | A cluster of entities and value objects treated as a single unit. One entity is the aggregate root — all external references go through it. |
| **Domain Event**   | Something meaningful that happened in the domain. Use for side effects and cross-boundary communication.                                     |
| **Repository**     | A collection-like interface for aggregate persistence. Hide I/O details behind it.                                                           |
| **Domain Service** | Stateless operations that don't naturally belong to an entity or value object.                                                               |

**Guidance**: Don't force DDD on CRUD features. If the feature is simple data entry with no business rules, a transaction script (command → Dapper query) is appropriate. Reserve DDD for features with significant business complexity.

#### CQRS (Command Query Responsibility Segregation)

- **Commands** mutate state. They return void or a success/failure result. Route through MediatR `IRequest<T>`.
- **Queries** return data. They never change state. Route through MediatR `IRequest<T>` with read-only connections.
- **For this codebase**: Simple CQRS — same database, same model. Separate read/write stacks only if performance demands it.

#### Separation of Concerns

| Layer                    | Concern                                                   | Must NOT Contain                                         |
| ------------------------ | --------------------------------------------------------- | -------------------------------------------------------- |
| **Domain**         | Business rules, entities, value objects                   | Database access, HTTP, JSON serialization, DI references |
| **Application**    | Use case orchestration, DTOs, validation                  | SQL queries, HTTP context, framework-specific code       |
| **Infrastructure** | Database access, external APIs, file I/O, messaging       | Business rules, domain logic                             |
| **Api**            | HTTP endpoint definitions, middleware, auth configuration | Business rules, database access                          |
| **Angular**        | UI rendering, user interaction                            | Direct API calls outside services, business rules        |

#### API Design Principles

- **RESTful by default.** Use resource-oriented URLs (`/api/tenants/{id}/users`), standard HTTP methods, and proper status codes.
- **Version the API.** Use the `x-api-version` header or URL path versioning.
- **Consistent error responses.** Use Problem Details (RFC 7807) for all error responses.
- **Idempotency for mutations.** `PUT` and `DELETE` must be idempotent. `POST` creates new resources.
- **Never expose domain entities in the API.** Map to DTOs at the Application layer boundary.

#### Database Design Principles

- **Normalize by default.** 3NF unless there's a measured performance reason to denormalize.
- **Every table has `TenantId`.** Enforce tenant isolation at the row level.
- **Use GUIDs for primary keys exposed to the client.** Integers for internal-only tables are acceptable.
- **No cascading deletes in the schema.** Handle referential integrity in application logic or stored procedures.
- **Index foreign keys and common query predicates.** But don't over-index — every index has a write cost.

#### Resilience Patterns

| Pattern                                  | Tool                                     | When to Apply                                                        |
| ---------------------------------------- | ---------------------------------------- | -------------------------------------------------------------------- |
| **Retry with exponential backoff** | Polly                                    | Transient failures (network blips, SQL deadlocks, throttling)        |
| **Circuit Breaker**                | Polly                                    | When a downstream service is failing and retries would make it worse |
| **Timeout**                        | `HttpClient.Timeout` / Polly           | Every external call — never wait indefinitely                       |
| **Bulkhead**                       | Separate thread pools / connection pools | Isolate resource-intensive operations from the main request pipeline |
| **Cache-Aside**                    | Redis +`IDistributedCache`             | Frequently read, infrequently changed data                           |

#### Security Principles

- **Least privilege.** Every service identity gets only the permissions it needs.
- **Defense in depth.** Validate at the edge (Front Door WAF), at the API (JWT validation, input validation), and at the data layer (parameterized queries, row-level security).
- **Never trust client input.** Validate in the backend even if the frontend validates.
- **Secrets never in code or config files.** Key Vault only.
- **HTTPS everywhere.** No unencrypted communication between services.

#### Observability Principles

Every feature must produce:
- **Structured logs** (Serilog → Cosmos DB) with `CorrelationId`, `TenantId`, and `UserId`
- **Metrics** for key operations (request duration, error rate, throughput)
- **Distributed traces** spanning the Angular SPA → API → database → background functions

### 3. Produce Architecture Artifacts

#### Required Outputs

Generate the following files in `output/`. Every file is a plain-text edit surface the user can review and modify.

##### `output/architecture-decision-record.md`

An Architecture Decision Record (ADR) for every significant decision that isn't obvious from the system-level architecture. Use this format:

```markdown
## ADR-###: [Title]

**Status**: Proposed | Accepted | Deprecated | Superseded
**Date**: YYYY-MM-DD
**Context**: What problem are we solving? What constraints apply?
**Decision**: What did we choose and why?
**Alternatives Considered**: What else did we evaluate? Why was it rejected?
**Consequences**: What gets easier? What gets harder? What follow-up is needed?
**Principles Applied**: Which principles drove this decision?
```

##### `output/component-design.md`

For each new or modified component (Angular components, API endpoints, background functions, database objects):

- **Responsibility** — what single job does it do?
- **Dependencies** — what does it need from other components?
- **Interface** — inputs, outputs, contracts (API schemas, method signatures, SQL parameters)
- **Error handling** — what can go wrong and how is it handled?
- **Tenant awareness** — how does it enforce tenant isolation?

##### `output/data-model.md`

- Entity-Relationship diagram or table descriptions for new/modified database objects
- Column definitions with types, nullability, and constraints
- Index strategy
- Migration path (new table? alter existing? data migration needed?)
- Tenant partitioning strategy

##### `output/sequence-diagrams.md`

Mermaid sequence diagrams for every non-trivial flow. At minimum:
- The happy path (success scenario)
- One error path
- Any async/event-driven flow

##### `output/component-interaction.md`

A Mermaid component diagram showing how the new feature's pieces fit into the existing system architecture. Use the established diagram style from `docs/system-architecture.md`.

---

## Outputs

| File                                       | Purpose                                                      |
| ------------------------------------------ | ------------------------------------------------------------ |
| `output/architecture-decision-record.md` | ADRs for significant architectural choices                   |
| `output/component-design.md`             | Detailed design of each component (frontend, backend, data)  |
| `output/data-model.md`                   | Database schema changes, index strategy, migration plan      |
| `output/sequence-diagrams.md`            | Mermaid sequence diagrams for key flows                      |
| `output/component-interaction.md`        | Mermaid diagram showing integration with the existing system |

---

## Quality Checklist

Before marking this stage complete, verify:

- [ ] Every output traces back to a requirement from Stage 01
- [ ] Every architectural decision cites at least one industry-recognized principle
- [ ] The design respects the existing technology stack (Angular 19+, .NET 10, Dapper, SQL Server, Azure)
- [ ] Multi-tenancy is addressed at every layer
- [ ] Error handling is specified — no "happy path only" designs
- [ ] Security concerns are addressed (auth, authorization, data protection)
- [ ] Observability touchpoints are identified for each component (logging, metrics, tracing)
- [ ] Database changes are idempotent and include a migration plan
- [ ] API contracts are versioned and use consistent error formats (RFC 7807)
- [ ] The design does not violate the layering rules of Clean Architecture
- [ ] All diagrams render correctly as Mermaid
- [ ] The user can open, read, and understand every output file without a decoder
