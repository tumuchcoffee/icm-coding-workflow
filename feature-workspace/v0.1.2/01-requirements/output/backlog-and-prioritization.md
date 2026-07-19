# Backlog & Prioritization — Code Initialization (v0.1.2)

**Date**: 2026-07-18
**Feature**: Code Initialization — project scaffolding, containers, and database setup
**Sources**: `../01-requirements/references/requirements.md`

---

## MoSCoW Prioritization

### Must Have (Non-negotiable — the version fails without these)

| Priority | ID      | Story Summary                                                  | Source            |
|----------|---------|----------------------------------------------------------------|--------------------|
| Must     | FR-001  | Run entire application stack from local system                 | requirements.md   |
| Must     | FR-002  | Sticky header with hamburger, title, and user profile icon     | requirements.md   |
| Must     | FR-003  | Slide-out navigation menu from the left                        | requirements.md   |
| Must     | FR-004  | Sticky footer with "Synergistic" text                            | requirements.md   |
| Must     | FR-006  | Content area with Angular Router outlet                        | requirements.md   |
| Must     | FR-007  | .NET API health check endpoint (`GET /api/health`)             | requirements.md   |
| Must     | FR-008  | SQL Server database with `SchemaVersion` tracking table        | requirements.md   |
| Must     | FR-009  | No authentication layer (explicitly scoped out)                | requirements.md   |
| Must     | FR-010  | Postman collection for health check endpoint                   | requirements.md   |
| Must     | FR-011  | PrimeNG as the default UI component library                    | requirements.md   |
| Must     | FR-012  | Angular latest LTS version (2026-07-18)                        | requirements.md   |

### Should Have (High importance — painful to omit, workaround exists)

| Priority | ID      | Story Summary                                                  | Source            |
|----------|---------|----------------------------------------------------------------|--------------------|
| Should   | FR-005  | Optional right-hand detail pane (150px default width)          | requirements.md   |

### Could Have (Nice to have — if time permits)

| Priority | ID      | Story Summary                                                  | Source            |
|----------|---------|----------------------------------------------------------------|--------------------|
| Could    | NFR-001 | Page load performance < 3s (inferred threshold)                | NFR doc           |
| Could    | NFR-003 | WCAG 2.1 AA accessibility audit                                | NFR doc           |

### Won't Have (This Time)

| Priority | ID      | Story Summary                                                  | Reason                          |
|----------|---------|----------------------------------------------------------------|---------------------------------|
| Won't    | —       | Authentication (Azure AD B2C / Entra ID)                       | Explicitly deferred (FR-009)   |
| Won't    | —       | Multi-tenant middleware                                        | Depends on auth                 |
| Won't    | —       | MediatR / Dapper / FluentValidation                            | No business logic in v0.1.2    |
| Won't    | —       | Feature-level components beyond shell layout                   | Future versions                 |
| Won't    | —       | Azure cloud deployment                                         | Local dev only for v0.1.2      |

---

## Suggested Implementation Order

1. **FR-012 + FR-011**: Bootstrap Angular project with latest LTS + install PrimeNG
2. **FR-002**: Header component (hamburger, title, profile icon)
3. **FR-003**: Slide-out menu component
4. **FR-004**: Footer component
5. **FR-006**: Content area + Router setup
6. **FR-007**: .NET API with health check endpoint
7. **FR-008**: SQL Server database + migration scripts
8. **FR-001**: Startup script tying all three tiers together
9. **FR-010**: Postman collection
10. **FR-005**: Detail pane (should have — can be deferred if time is tight)
11. **FR-009**: Verification that no auth exists (not a build task, a validation check)

---

## Traceability Matrix

| Requirement ID | Source File           | Section in Source          |
|----------------|-----------------------|----------------------------|
| FR-001         | requirements.md       | (cross-cutting)            |
| FR-002         | requirements.md       | Angular — Header           |
| FR-003         | requirements.md       | Angular — Drop-down menu   |
| FR-004         | requirements.md       | Angular — Footer           |
| FR-005         | requirements.md       | Angular — Detail pane      |
| FR-006         | requirements.md       | Angular — Content area     |
| FR-007         | requirements.md       | .NET API                   |
| FR-008         | requirements.md       | Database                   |
| FR-009         | requirements.md       | Angular (auth statement)   |
| FR-010         | requirements.md       | Testing                    |
| FR-011         | requirements.md       | Angular (PrimeNG)          |
| FR-012         | requirements.md       | Angular (LTS version)      |