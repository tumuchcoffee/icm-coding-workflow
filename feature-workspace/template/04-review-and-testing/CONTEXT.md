# Review & Testing Specifications

## Stage Purpose

Validate the implementation produced by [Stage 03](../03-implementation/output/) through systematic code review and comprehensive test execution. This stage acts as the quality gate before the feature is considered complete. Every defect found must be documented; every test result must be reported. The single consolidated deliverable — including code review findings and test results — is saved to `.\output\review-and-test-results.md`.

---

## Inputs

| Layer                         | Source                                          | What to Load                                                                                              |
| ----------------------------- | ----------------------------------------------- | --------------------------------------------------------------------------------------------------------- |
| **Layer 3 (Reference)** | `../../docs/coding-standards.md`              | Language and platform conventions for C#, Angular/TypeScript, SQL Server, and Azure                       |
| **Layer 3 (Reference)** | `../../docs/system-architecture.md`           | System-level architecture principles, component boundaries, tech stack, solution structure                |
| **Layer 3 (Reference)** | `../../docs/project-management.md`            | Requirements traceability standards, validation vs. verification principles                               |
| **Layer 4 (Implementation)** | `../03-implementation/output/*.md`            | The implementation results manifest — all files created/modified, traceability matrix, handoff notes       |
| **Layer 4 (Working)**   | `./references/*.*`                            | Supplementary review criteria, test plans, expected behavior specifications, or third-party test docs     |

> **Important**: Read every file matching `../../docs/*.md` and `../03-implementation/output/*.md`. Do not skip any. If the implementation output directory is empty or missing expected files, stop and report the gap — do not guess. For `./references/`, load all files present; an empty references folder is acceptable if no supplementary review criteria were provided.

---

## Process

### 1. Ingest All Inputs

1. **Load all reference documents.** Read every file in `../../docs/`. Build a checklist of every standard, convention, and architectural rule that applies to this review.
2. **Load the implementation results.** Read `../03-implementation/output/*.md` in full. Identify:
   - **File manifest** — every file created or modified, organized by layer and project.
   - **Architecture traceability matrix** — every ADR and component that was implemented.
   - **Coding standards compliance grid** — the implementation team's self-assessment.
   - **Test coverage summary** — unit, integration, and E2E test counts by layer.
   - **Deviations & rationale** — intentional divergences from the architecture.
   - **Known issues & open items** — items the implementation team flagged.
   - **Stage 04 handoff notes** — which files to review first, test commands to run, smoke test steps, and environment prerequisites.
3. **Load supplementary references.** Scan `./references/` and load every file. Note any additional review checklists, test plans, expected-behavior documents, or third-party testing guidance.

### 2. Perform Code Review

Review every file listed in the implementation manifest against the reference standards. Organize findings into these categories:

#### 2.1 Architecture Compliance

For each file, verify:

- **Clean Architecture boundaries.** Do dependencies point inward? Does the Domain layer reference only itself? Does the Application layer avoid Infrastructure references? Does the Api layer contain no business logic?
- **SOLID principles.** Is each class single-purpose? Are abstractions used instead of concretions? Are interfaces small and role-focused?
- **Multi-tenancy.** Does every database table include `TenantId`? Does every API request carry and validate `X-Tenant-Id`? Do repository queries filter by tenant?
- **API design.** Are endpoints RESTful? Are error responses Problem Details (RFC 7807)? Are domain entities mapped to DTOs at the boundary? Is versioning handled?

#### 2.2 Coding Standards Compliance

For each language used, verify against the rules in `../../docs/coding-standards.md`:

- **C#**: PascalCase for public members, explicit access modifiers, async/await for I/O, nullable reference types enabled, expression-bodied members where appropriate, usings organized.
- **Angular/TypeScript**: Standalone components, Signals for state, strong typing (no `any`), `async` pipe for subscriptions, feature-based folder structure, ESLint conformance.
- **SQL Server**: PascalCase for object names, schema-qualified references, parameterized queries (never concatenated input), idempotent scripts, indexed foreign keys and query predicates.
- **Azure/IaC**: Bicep or Terraform provisioning, Key Vault for secrets, Managed Identity for service auth, resource tagging, least-privilege RBAC.

#### 2.3 Security Review

- Are secrets, connection strings, and keys absent from code and config files?
- Are all database queries parameterized? (Zero tolerance for string concatenation of user input.)
- Do endpoints include `RequireAuthorization()` unless explicitly public?
- Are external HTTP calls protected by Polly resilience policies (retry, circuit breaker, timeout)?
- Is input validated (FluentValidation on every command/query)?

#### 2.4 Error Handling & Observability

- Is error handling explicit on every code path? Are there any unhandled exception risks?
- Do structured logs include `CorrelationId`, `TenantId`, and `UserId`?
- Are metrics emitted for key operations?
- Are idempotent operations (PUT, DELETE, SQL scripts) truly idempotent?

#### 2.5 Test Quality

Review the tests themselves for quality:

- Do tests follow the `MethodName_Scenario_ExpectedBehavior` naming convention?
- Do tests follow the AAA pattern (Arrange, Act, Assert)?
- Are tests independent — no shared state, no ordering assumptions?
- Do tests cover both happy paths and error paths from the sequence diagrams?
- Are edge cases tested (null/empty inputs, boundary values, tenant isolation)?

### 3. Execute Tests

Run every test suite identified in the implementation results. Follow this sequence:

#### 3.1 Backend Tests (.NET)

Run in order:
1. **Domain unit tests** — `dotnet test tests/Domain.UnitTests/`
2. **Application unit tests** — `dotnet test tests/Application.UnitTests/`
3. **Infrastructure integration tests** — `dotnet test tests/Infrastructure.IntegrationTests/`
4. **API integration tests** — `dotnet test tests/Api.IntegrationTests/`

For each suite, capture:
- Total tests, passed, failed, skipped.
- Duration.
- Any failure messages (full stack traces where relevant).
- Whether Testcontainers or other infrastructure dependencies started correctly.

#### 3.2 Frontend Tests (Angular)

Run in order:
1. **Unit tests** — `npx jest` (or `ng test`)
2. **E2E tests** — `npx playwright test`

For each suite, capture:
- Total tests, passed, failed, skipped.
- Duration.
- Any failure messages (full stack traces where relevant).

#### 3.3 Database Tests

If the implementation includes SQL test scripts or database integration tests, run them and capture results.

#### 3.4 Smoke Tests

Execute the smoke test steps provided in the implementation handoff notes. These are the minimum actions to verify the feature works end-to-end. Document each step's outcome (pass/fail) with any observed behavior.

### 4. Write the Review & Test Results

Save to `.\output\review-and-test-results.md` using the structure below:

```markdown
# Review & Test Results

**Feature**: [Feature Name]
**Date**: YYYY-MM-DD
**Implementation Reference**: ../03-implementation/output/implementation-results.md
**Reviewer**: AI Agent (Stage 04)

---

## 1. Review Summary

- **Total files reviewed**: ##
- **Architecture compliance**: ✅ Pass / ⚠️ Issues found / ❌ Blocking issues
- **Coding standards compliance**: ✅ Pass / ⚠️ Issues found / ❌ Blocking issues
- **Security review**: ✅ Pass / ⚠️ Issues found / ❌ Blocking issues
- **Overall review verdict**: ✅ Approved / ⚠️ Conditional (issues must be resolved) / ❌ Rejected

---

## 2. Code Review Findings

### 2.1 Architecture Compliance

| # | File | Layer | Issue | Severity | Principle Violated | Recommendation |
|---|------|-------|-------|----------|-------------------|----------------|
| 1 | `src/02-backend/Api/Endpoints/...` | Api | Business logic in endpoint | 🔴 Critical | Separation of Concerns | Move logic to Application layer handler |
| ... | ... | ... | ... | ... | ... | ... |

**Severity legend**:
- 🔴 **Critical** — must be fixed before merge (security risk, data corruption, architecture violation).
- 🟡 **Warning** — should be fixed; acceptable with documented rationale.
- 🔵 **Info** — minor improvement opportunity; non-blocking.

### 2.2 Coding Standards Compliance

| # | File | Language | Issue | Standard Reference | Recommendation |
|---|------|----------|-------|-------------------|----------------|
| 1 | `src/01-ui/...` | TypeScript | `any` type used | coding-standards.md#Angular | Replace with explicit type or `unknown` |
| ... | ... | ... | ... | ... | ... |

### 2.3 Security Review

| # | File | Issue | Severity | Recommendation |
|---|------|-------|----------|----------------|
| 1 | ... | ... | ... | ... |

### 2.4 Error Handling & Observability

| # | File | Issue | Severity | Recommendation |
|---|------|-------|----------|----------------|
| 1 | ... | ... | ... | ... |

### 2.5 Test Quality

| # | File | Issue | Severity | Recommendation |
|---|------|-------|----------|----------------|
| 1 | ... | ... | ... | ... |

---

## 3. Test Results

### 3.1 Backend Tests

| Test Suite | Total | Passed | Failed | Skipped | Duration | Status |
|-----------|-------|--------|--------|---------|----------|--------|
| Domain Unit Tests | ## | ## | ## | ## | #.#s | ✅/❌ |
| Application Unit Tests | ## | ## | ## | ## | #.#s | ✅/❌ |
| Infrastructure Integration Tests | ## | ## | ## | ## | #.#s | ✅/❌ |
| API Integration Tests | ## | ## | ## | ## | #.#s | ✅/❌ |
| **Backend Total** | **##** | **##** | **##** | **##** | **#.#s** | **✅/❌** |

#### Failed Test Details (Backend)

<details>
<summary>Test name — Failure message</summary>

```
Full stack trace and error message
```

</details>

### 3.2 Frontend Tests

| Test Suite | Total | Passed | Failed | Skipped | Duration | Status |
|-----------|-------|--------|--------|---------|----------|--------|
| Unit Tests (Jest) | ## | ## | ## | ## | #.#s | ✅/❌ |
| E2E Tests (Playwright) | ## | ## | ## | ## | #.#s | ✅/❌ |
| **Frontend Total** | **##** | **##** | **##** | **##** | **#.#s** | **✅/❌** |

#### Failed Test Details (Frontend)

<details>
<summary>Test name — Failure message</summary>

```
Full stack trace and error message
```

</details>

### 3.3 Database Tests

| Test Suite | Total | Passed | Failed | Skipped | Duration | Status |
|-----------|-------|--------|--------|---------|----------|--------|
| SQL Tests | ## | ## | ## | ## | #.#s | ✅/❌ |

#### Failed Test Details (Database)

<details>
<summary>Test name — Failure message</summary>

```
Full stack trace and error message
```

</details>

### 3.4 Smoke Tests

| # | Step | Expected | Actual | Status |
|---|------|----------|--------|--------|
| 1 | [Step description] | [Expected behavior] | [Observed behavior] | ✅/❌ |
| ... | ... | ... | ... | ... |

### 3.5 Overall Test Verdict

- **Total tests across all suites**: ##
- **Passed**: ## (##.#%)
- **Failed**: ##
- **Skipped**: ##
- **Overall**: ✅ All passing / ⚠️ Failures present / ❌ Blocking failures

---

## 4. Architecture Traceability Verification

Cross-reference the implementation against the architecture. Every architecture decision and component must be verified as implemented.

| Architecture Artifact | Decision / Component | Expected In (per arch) | Found In (per review) | Status |
|-----------------------|---------------------|----------------------|---------------------|--------|
| ADR-001 | ... | ... | ... | ✅ Verified / ⚠️ Mismatch / ❌ Missing |
| Component: ... | ... | ... | ... | ✅ Verified / ⚠️ Mismatch / ❌ Missing |

---

## 5. Traceability to Requirements

Verify that every requirement from Stage 01 is covered by at least one test.

| Requirement ID | Requirement Summary | Covered By Test(s) | Status |
|---------------|-------------------|-------------------|--------|
| FR-001 | ... | Test name(s) | ✅ Covered / ⚠️ Partial / ❌ Not covered |
| NFR-001 | ... | Test name(s) | ✅ Covered / ⚠️ Partial / ❌ Not covered |

---

## 6. Issues Summary

### 6.1 Blocking Issues (Must Fix Before Merge)

| # | Category | File | Description | Severity |
|---|----------|------|-------------|----------|
| 1 | ... | ... | ... | 🔴 Critical |

### 6.2 Warnings (Should Fix)

| # | Category | File | Description | Severity |
|---|----------|------|-------------|----------|
| 1 | ... | ... | ... | 🟡 Warning |

### 6.3 Observations (Non-Blocking)

| # | Category | File | Description | Severity |
|---|----------|------|-------------|----------|
| 1 | ... | ... | ... | 🔵 Info |

---

## 7. Final Verdict

- **Code Review**: ✅ Approved / ⚠️ Conditional / ❌ Rejected
- **Test Results**: ✅ All passing / ⚠️ Failures present / ❌ Blocking failures
- **Overall**: ✅ Ready for merge / ⚠️ Changes required / ❌ Do not merge

---

## 8. Sources

| File | Type | Summary |
|------|------|---------|
| `../../docs/coding-standards.md` | Reference | Coding standards applied during review |
| `../../docs/system-architecture.md` | Reference | Architecture rules applied during review |
| `../../docs/project-management.md` | Reference | Traceability and verification standards |
| `../03-implementation/output/*.md` | Working | Implementation manifest and handoff notes |
| `./references/*` | Working | Supplementary review criteria |
```

---

## Outputs

| File | Purpose |
|------|---------|
| `output/review-and-test-results.md` | Consolidated code review findings, test execution results, traceability verification, and final merge recommendation — the single required deliverable for this stage |

---

## Quality Checklist

Before marking this stage complete, verify:

- [ ] Every file in the implementation manifest has been reviewed against architecture and coding standards
- [ ] All five code review categories (Architecture, Coding Standards, Security, Error Handling, Test Quality) have been addressed
- [ ] Every test suite identified in the implementation results has been executed
- [ ] Test results include totals, pass/fail/skip counts, durations, and failure details with stack traces
- [ ] All smoke test steps from the handoff notes have been executed and documented
- [ ] Architecture traceability has been verified — every ADR and component maps to actual implementation
- [ ] Requirements traceability has been verified — every requirement has at least one corresponding test
- [ ] Every finding is classified with the correct severity (Critical, Warning, Info)
- [ ] The final verdict is clear and unambiguous (Approved, Conditional, or Rejected)
- [ ] The results file is self-contained — a reader can understand all findings without opening individual source files
- [ ] All source files used as input are listed in the Sources section with a one-line summary