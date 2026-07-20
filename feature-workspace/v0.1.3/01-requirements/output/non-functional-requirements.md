# Non-Functional Requirements — Docker Local Development

> **Tier:** Lightweight — only the 1–2 NFRs that matter are documented.
> **Sources:** `references/requirements.md`
> **Generated:** 2026-07-20

---

## NFR-001: Cold Start Time
**Category:** Performance
**Threshold:** Full stack (`docker compose up` from a cold state) reaches healthy status within 120 seconds on a developer machine with 16 GB RAM and SSD storage.
**Measurement:** Timed `docker compose up` run; health check on all three containers must pass.
**Priority:** Should Have

**Sources:** requirements.md (implicit — developer productivity)

---

## NFR-002: Container Image Sizes
**Category:** Maintainability / Resource Efficiency
**Threshold:**
- UI image: < 500 MB (multi-stage build, production-slim base)
- API image: < 300 MB (ASP.NET runtime image, self-contained trimmed)
- SQL Server: uses official `mcr.microsoft.com/mssql/server:2022-latest` (no custom threshold)
**Measurement:** `docker image ls` after build.
**Priority:** Should Have

**Sources:** requirements.md (implicit — developer onboarding speed)

---

## NFR-003: Port Isolation
**Category:** Security
**Threshold:** Only ports 4200 (UI) and 5001 (API) are exposed to the host. The SQL Server port (1433) is accessible only on the internal Docker network unless explicitly overridden.
**Measurement:** `docker compose ps` and `docker network inspect` to verify port bindings.
**Priority:** Must Have

**Sources:** requirements.md ("external access from outside the containers" for UI/API; database access is internal only)

---

## NFR-004: Cross-Platform Compatibility
**Category:** Usability
**Threshold:** `docker compose up` must succeed on Windows (Docker Desktop), macOS (Docker Desktop), and Linux (Docker Engine) without platform-specific modifications.
**Measurement:** Verified by running the compose stack on each OS.
**Priority:** Should Have

**Sources:** requirements.md (implicit — "a developer" is not OS-constrained)

---

## NFR Checklist Summary

| Category        | Assessed? | NFR Defined? |
|-----------------|-----------|--------------|
| Performance     | ✅        | NFR-001      |
| Security        | ✅        | NFR-003      |
| Availability    | N/A       | —            |
| Scalability     | N/A       | —            |
| Maintainability | ✅        | NFR-002      |
| Usability       | ✅        | NFR-004      |
| Compliance      | N/A       | —            |

---

## Gaps

| ID | Description |
|----|-------------|
| NFR-GAP-001 | No explicit performance budget in source; NFR-001 threshold is a reasonable default. Needs stakeholder confirmation. |
| NFR-GAP-002 | No explicit image size budget in source; NFR-002 thresholds are reasonable defaults for dev tooling. Needs stakeholder confirmation. |