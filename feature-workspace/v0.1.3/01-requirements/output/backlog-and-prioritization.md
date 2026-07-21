# Backlog & Prioritization — Docker Local Development

> **Tier:** Lightweight — MoSCoW on the release backlog.
> **Sources:** `references/requirements.md`
> **Generated:** 2026-07-20

---

## MoSCoW-Prioritized Backlog

| Priority | ID      | Story Summary                                      | Source          |
|----------|---------|----------------------------------------------------|-----------------|
| Must     | FR-001  | Dockerfile for Angular UI                          | requirements.md |
| Must     | FR-002  | Dockerfile for .NET API                            | requirements.md |
| Must     | FR-003  | Docker Compose service for SQL Server              | requirements.md |
| Must     | FR-004  | Single `docker-compose.yml` at repo root           | requirements.md |
| Must     | FR-005  | Shared Docker network for all containers           | requirements.md |
| Must     | FR-006  | External access to UI (4200) and API (5001)        | requirements.md |
| Must     | FR-007  | Database migrations on API startup (DbUp)          | requirements.md |
| Should   | FR-008  | Source code volume mounts for hot reload           | requirements.md |

---

## Implementation Order

```
Phase 1 (Foundation)
  FR-001 → FR-002 → FR-003
  (Individual Dockerfiles / service definitions)

Phase 2 (Orchestration)
  FR-004 → FR-005 → FR-006 → FR-007
  (Compose file, networking, ports, migrations)

Phase 3 (Developer Experience)
  FR-008
  (Volume mounts, hot reload)
```

---

## Traceability Matrix

| Story ID | Source File        | Source Excerpt / Signal                                             |
|----------|--------------------|---------------------------------------------------------------------|
| FR-001   | requirements.md    | "allow a developer to run the code in a Docker container"           |
| FR-002   | requirements.md    | "allow a developer to run the code in a Docker container"           |
| FR-003   | requirements.md    | "run the code in a Docker container" (database is required)         |
| FR-004   | requirements.md    | "Group the containers in a single docker network" (implies Compose) |
| FR-005   | requirements.md    | "Group the containers in a single docker network"                   |
| FR-006   | requirements.md    | "allowing external access from outside the containers"              |
| FR-007   | requirements.md    | Implicit — `001_CreateSchemaVersion.sql` + DbUp pattern             |
| FR-008   | requirements.md    | "run or confirm their changes" (implies developer iteration)        |

---

## Out of Scope (for this feature)

- Production Docker deployment (CI/CD pipeline, Kubernetes, etc.)
- Docker Swarm or multi-host orchestration
- Container registry push/pull
- Docker Compose profiles for production vs. development
- Health check endpoints beyond basic container health