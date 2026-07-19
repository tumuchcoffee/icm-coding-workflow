# Non-Functional Requirements — Code Initialization (v0.1.2)

**Date**: 2026-07-18
**Feature**: Code Initialization — project scaffolding, containers, and database setup
**Sources**: `../01-requirements/references/requirements.md`
**Note**: The source requirements contain no explicit NFRs. The following are **default thresholds** derived from standard web application expectations and marked as "Inferred" until confirmed by stakeholders.

---

## NFR-001: Page Load Performance — Initial Shell
**Category**: Performance  
**Threshold**: Initial page load (uncached, first visit) < 3 seconds at p95 on a 10 Mbps connection  
**Measurement**: Lighthouse Performance audit via Chrome DevTools or CI pipeline  
**Priority**: Should Have  
**Status**: ⚠ Inferred — not stated in source requirements  
**Sources:** requirements.md (implicit — Angular SPA shell)

---

## NFR-002: API Response Time — Health Check
**Category**: Performance  
**Threshold**: `GET /api/health` responds in < 100 ms at p95 (no database call in v0.1.2)  
**Measurement**: Response time header or middleware timer  
**Priority**: Should Have  
**Status**: ⚠ Inferred — not stated in source requirements  
**Sources:** requirements.md (.NET API section)

---

## NFR-003: Accessibility — WCAG 2.1 Level AA
**Category**: Usability / Compliance  
**Threshold**: All UI components pass WCAG 2.1 AA automated checks (axe-core or Lighthouse)  
**Measurement**: axe-core audit in CI pipeline; manual keyboard navigation test  
**Priority**: Should Have  
**Status**: ⚠ Inferred — not stated in source requirements; PrimeNG components ship with baseline accessibility  
**Sources:** requirements.md (implicit — admin panel UI)

---

## NFR-004: HTTPS Everywhere
**Category**: Security  
**Threshold**: All communication between Angular SPA and .NET API uses HTTPS (even in development)  
**Measurement**: Browser security panel; `curl -k` verification  
**Priority**: Should Have  
**Status**: ⚠ Inferred — not stated in source requirements  
**Sources:** requirements.md (implicit — standard web application practice)

---

## NFR-005: Local-Only Deployment (v0.1.2)
**Category**: Deployment / Scalability  
**Threshold**: The entire stack (Angular, .NET API, SQL Server) runs on a single developer machine with no Azure or cloud dependencies  
**Measurement**: Developer can run all three tiers after `git clone` + `run.ps1`  
**Priority**: Must Have  
**Status**: ✅ Explicit — "Create the project for the angular code... Create a SQL Server database... on a local development machine"  
**Sources:** requirements.md (Angular, .NET API, Database sections)

---

## NFR-006: Database — No Sensitive Data in v0.1.2
**Category**: Security / Compliance  
**Threshold**: The v0.1.2 database contains zero user data — only the `SchemaVersion` migration tracking table  
**Measurement**: Schema inspection; data classification scan  
**Priority**: Must Have  
**Status**: ✅ Explicit — no tables beyond schema versioning defined  
**Sources:** requirements.md (Database section)

---

## NFR Checklist Summary

| NFR Category      | Assessed? | Quantified? | Notes                                      |
|--------------------|-----------|-------------|--------------------------------------------|
| Performance        | ✅        | ✅          | Shell < 3s, health check < 100ms (inferred) |
| Security           | ✅        | Partial     | HTTPS + no auth + no user data             |
| Availability       | ❌        | —           | Not applicable (local dev only)            |
| Scalability        | ❌        | —           | Not applicable (local dev only)            |
| Maintainability    | ✅        | ✅          | Standalone components, Clean Architecture  |
| Usability          | ✅        | ✅          | WCAG 2.1 AA baseline (inferred)            |
| Compliance         | ✅        | ✅          | No PII/data in v0.1.2                      |
| Disaster Recovery  | ❌        | —           | Not applicable (local dev only)            |
| Observability      | ❌        | —           | Deferred to future versions                |

---

## Gaps Requiring Stakeholder Decisions

| # | Gap | Impact |
|---|-----|--------|
| G-001 | No explicit performance SLA defined | Accept defaults or define with team |
| G-002 | No browser support matrix | Default to latest 2 versions of Chrome, Edge, Firefox, Safari |
| G-003 | No accessibility standard explicitly stated | Adopt WCAG 2.1 AA as baseline; confirm with stakeholders |