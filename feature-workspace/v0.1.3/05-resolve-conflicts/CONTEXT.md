# Resolve Conflicts — Best Practices, Standards & Conventions

## Stage Purpose

Resolve all merge conflicts that arose during the implementation and review stages. This stage ensures that divergent code changes are integrated cleanly, preserving correctness, architectural integrity, and coding standards across every file. The single consolidated deliverable — including a conflict inventory, resolution log, and post-resolution validation results — is saved to `.\output\resolve-conflicts-report.md`.

---

## Inputs

| Layer                         | Source                                              | What to Load                                                                                              |
| ----------------------------- | --------------------------------------------------- | --------------------------------------------------------------------------------------------------------- |
| **Layer 3 (Reference)** | `../../docs/coding-standards.md`                  | Language and platform conventions for C#, Angular/TypeScript, SQL Server, and Azure                       |
| **Layer 3 (Reference)** | `../../docs/system-architecture.md`               | System-level architecture principles, component boundaries, tech stack, solution structure                |
| **Layer 3 (Reference)** | `../../docs/project-management.md`                | Requirements traceability, validation vs. verification principles, stakeholder communication guidelines   |
| **Layer 4 (Requirements)**    | `../01-requirements/output/feature-requirements.md` | Functional requirements with GIVEN/WHEN/THEN acceptance criteria — the "what" that must remain correct     |
| **Layer 4 (Requirements)**    | `../01-requirements/output/non-functional-requirements.md` | Quantified NFR thresholds — performance, security, and compliance targets must not regress                |
| **Layer 4 (Requirements)**    | `../01-requirements/output/backlog-and-prioritization.md` | MoSCoW-prioritized backlog — guides which side to favor when a conflict touches a Must Have story         |
| **Layer 4 (Requirements)**    | `../01-requirements/output/glossary.md`            | Domain terminology — ensures consistent naming across merged code                                         |
| **Layer 4 (Architecture)**    | `../02-architecture/output/*.md`                   | Architecture Decision Records, component design, data model, sequence diagrams — the blueprint to uphold  |
| **Layer 4 (Implementation)**  | `../03-implementation/output/implementation-results.md` | File manifest and traceability matrix from the implementation stage                                       |
| **Layer 4 (Review)**          | `../04-review-and-testing/output/review-and-test-results.md` | Code review findings and test results — known issues must not be re-introduced or exacerbated             |
| **Layer 4 (Working)**         | `./references/*.*`                                  | Supplementary conflict-resolution guidance, merge checklists, third-party tool documentation              |

> **Important**: Read every file matching the paths above. Do not skip any. If a required input is empty or missing, note it as a gap in the report — do not guess. For `./references/`, load all files present; an empty references folder is acceptable.

---

## Process

### 1. Ingest All Inputs

1. **Load reference documents.** Read every file in `../../docs/` in full. Build a checklist of every standard, convention, and architectural rule that must be preserved through conflict resolution.
2. **Load requirements.** Read every file in `../01-requirements/output/`. Identify which user stories and NFRs are in scope — these define what behavior must not be broken by any merge resolution.
3. **Load architecture blueprints.** Scan `../02-architecture/output/` and load every `.md` file. Identify ADR decisions, component specs, data model constraints, and sequence flows that the merged code must honor.
4. **Load implementation results.** Read `../03-implementation/output/implementation-results.md`. Note the file manifest — every file created or modified — and the traceability matrix linking code to requirements.
5. **Load review & test results.** Read `../04-review-and-testing/output/review-and-test-results.md`. Note any unresolved defects, known issues, or test failures. Conflict resolution must not silently reintroduce or worsen these.
6. **Load supplementary references.** Scan `./references/` and load every file. Note any merge checklists, tool-specific guidance, or team-defined resolution policies.

### 2. Pre-Merge Preparation

Before resolving any conflict, complete these preparatory steps:

#### 2.1 Establish a Clean Baseline

- **Pull the latest from all branches involved in the merge.** Stale local copies are the #1 cause of unnecessary conflicts and incorrect resolutions.
- **Verify the build passes on the target branch before merging.** If the target branch is broken, fix it first — do not merge into a broken base.
- **Run the full test suite on the target branch.** Record the results as a pre-merge baseline for comparison after resolution.

#### 2.2 Identify All Affected Parties

- Map every conflicting file to its owning team or developer using the implementation manifest from Stage 03.
- For each conflict, identify the two (or more) authors whose changes overlap. Both perspectives are needed for informed resolution.
- If a conflict spans multiple repositories or services, coordinate across team boundaries — do not resolve cross-cutting conflicts in isolation.

#### 2.3 Classify Every Conflict

Categorize each conflict before attempting resolution. Use this taxonomy:

| Conflict Type | Description | Resolution Strategy |
|---|---|---|
| **Textual / trivial** | Whitespace, formatting, import ordering, comment changes | Apply the project's formatter/linter; accept whichever side matches coding standards. Do not spend time debating formatting. |
| **Additive / non-overlapping** | Both branches added different code to different regions of the same file | Accept both sides. Verify they don't interact unexpectedly. |
| **Semantic / overlapping** | Both branches modified the same logic, same method, or same region | Requires human judgment. Understand the intent of both changes before resolving. See Section 3. |
| **Architectural / structural** | File renames, directory moves, project restructures, namespace changes | Resolve the structural conflict first, then resolve any content conflicts in the moved/renamed files. |
| **Data / schema** | Database migration version numbers, schema changes, seed data conflicts | Favor the higher migration sequence number. Merge schema changes carefully — never silently drop a column or constraint. See Section 3.2 (SQL Server). |
| **Dependency** | Package version conflicts (package.json, .csproj, docker-compose.yml) | Favor the higher compatible version. Test for breaking changes. |
| **Generated code** | Auto-generated files (OpenAPI clients, migration designer files, snapshot files) | Re-generate after merging source files. Never hand-edit generated code. |

Document every conflict with its type, files affected, and authors in the resolution report.

### 3. Resolution Standards & Best Practices

#### 3.1 General Resolution Principles

Apply these principles to every conflict, regardless of language or technology:

- **Understand before you act.** Read both sides of every conflict in full. Do not blindly accept "theirs" or "ours." If you don't understand why a change was made, contact the author before resolving.
- **Preserve intent, not just text.** The goal is not to make the merge markers disappear — it is to ensure the merged code correctly implements the intended behavior from both branches. If both changes are needed, the resolution must incorporate both.
- **One conflict, one commit.** Resolve conflicts in logical, atomic commits. Do not bundle unrelated conflict resolutions into a single "fix merge conflicts" commit. Each resolution should be traceable to the specific conflict it addressed.
- **Favor the side that aligns with requirements.** When two changes are mutually exclusive, refer back to `../01-requirements/output/`. The side that satisfies a Must Have requirement takes precedence over one that satisfies a Should or Could.
- **Favor the side that aligns with architecture.** When requirements don't provide a clear answer, refer to `../02-architecture/output/`. The change that conforms to the ADRs and component design takes precedence.
- **When in doubt, communicate.** If neither requirements nor architecture provide clarity, do not guess. Flag the conflict, propose both options with tradeoffs, and escalate to the feature lead or product owner for a decision. Document the escalation and its outcome in the report.
- **Tests are non-negotiable.** If one branch added tests for its changes and the other didn't, the merged code must include those tests (updated for the merged logic). If both branches added tests that now conflict, resolve the test conflicts with the same care as production code.
- **Never resolve a conflict by deleting code you don't understand.** If you're unsure what a block of code does, deleting it to make the merge marker go away is the worst possible resolution. Investigate or escalate.

#### 3.2 Language-Specific Conflict Resolution

##### C# Conflicts

- **Namespace and using conflicts.** Merge all unique usings from both sides. Remove unused usings after resolution. Place usings inside the namespace declaration per `../../docs/coding-standards.md`.
- **Method signature conflicts.** If both branches changed the same method's signature, the resolution must produce a single, correct signature. Check all call sites — a signature change in one branch may have callers that the other branch doesn't know about.
- **Dependency injection registration.** If both branches added `AddScoped`/`AddTransient`/`AddSingleton` calls, preserve both. If both registered the same interface to different implementations, this is a true conflict — only one implementation wins. Flag this for discussion.
- **Nullable reference type annotations.** If one branch added `?` annotations and the other didn't, merge toward the safer (more nullable-aware) version. The merged code must compile with nullable reference types enabled.
- **Async/await patterns.** If one branch converted synchronous code to async and the other modified the synchronous version, the resolution must produce async code. Never downgrade async to sync to resolve a conflict.

##### Angular / TypeScript Conflicts

- **Component template vs. logic conflicts.** If one branch changed the `.component.html` template and the other changed the `.component.ts` logic, verify that the merged version is internally consistent — the template must reference properties and methods that exist in the merged class.
- **Signal vs. traditional state.** If one branch migrated to Signals (`signal()`, `computed()`) and the other added new state using traditional patterns (`BehaviorSubject`), resolve toward Signals per the architecture decision for Angular 19+.
- **Import conflicts.** Merge all unique imports. Run the TypeScript compiler and ESLint after resolution. Remove any unused imports. Verify no circular dependencies were introduced.
- **Route configuration conflicts.** If both branches added routes, preserve both. If both modified the same route, merge carefully — verify the route path, component, guards, and data resolvers are all correct.
- **CSS/SCSS conflicts.** Merge both sets of styles. Check for selector conflicts — if both branches styled the same element differently, decide which style is correct based on the requirements. Run a visual review of the affected components.

##### SQL Server Conflicts

- **Migration version numbering.** If both branches created a migration with the same sequence number, the branch merged second must renumber its migration to the next available number. Never skip numbers or create gaps.
- **Schema changes to the same table.** If both branches added columns to the same table, preserve both additions. If both modified the same column (data type, nullability, default), review the intent of both changes — the more permissive type wins (e.g., `NVARCHAR(200)` over `NVARCHAR(100)`, `NULL` over `NOT NULL` only if both changes can tolerate it). When in doubt, flag for discussion.
- **Stored procedure and function changes.** If both branches modified the same stored procedure, the resolution must produce a single procedure that incorporates the logic from both branches. Test the merged procedure against all expected inputs from both branches.
- **Index changes.** Preserve all non-duplicate indexes. If both branches created an index on the same column with different options (e.g., `INCLUDE` columns, `WHERE` clause), merge the options — the broader index usually subsumes the narrower one.
- **Seed/reference data.** If both branches inserted different seed data, preserve both sets. If both modified the same seed row, the later change (by timestamp or business logic) takes precedence. Document any data conflicts in the report.

##### Azure / Infrastructure as Code (Bicep/Terraform) Conflicts

- **Resource definition conflicts.** If both branches modified the same resource, merge the properties carefully. Conflicting properties (different SKUs, different regions) must be resolved by referencing the architecture decisions and NFRs.
- **Naming convention conflicts.** Resource names must follow the naming convention from `../../docs/coding-standards.md` (environment, region, instance). The resolution must produce a compliant name.
- **Secret and configuration conflicts.** Never resolve a conflict by exposing a secret or connection string that one branch had moved to Key Vault. The merged code must reference Key Vault.
- **Role assignment conflicts.** Preserve all unique role assignments from both branches. If both branches assigned different roles to the same principal, preserve the union (principle of least privilege — if roles overlap, the more permissive may be intentional; flag for review).

#### 3.3 Architectural Integrity During Resolution

- **Layer boundaries must remain intact.** After resolving a conflict, verify that the Domain layer still references only itself, the Application layer references only Domain, and Infrastructure references only Application interfaces. A merge must never create a dependency that points in the wrong direction.
- **SOLID principles must be preserved.** If resolution requires modifying a class, re-verify: Does it still have a single responsibility? Is it open for extension but closed for modification? Are dependencies inverted?
- **Multi-tenancy must not be broken.** Every resolved database query must still filter by `TenantId`. Every resolved API endpoint must still carry and validate `X-Tenant-Id`. If one branch added tenant-awareness and the other didn't, resolve toward tenant-awareness.
- **API contracts must remain stable.** If one branch changed an API endpoint's request or response shape, verify that all consumers of that endpoint (including the Angular frontend) are updated consistently. A mismatch between API and client is a latent defect.

#### 3.4 Communication & Collaboration

- **Notify affected authors before resolving their code.** If you are resolving a conflict that touches code written by someone else, inform them of the resolution — ideally through the pull request review process.
- **Use the PR as the communication hub.** Every non-trivial conflict resolution should be explained in a PR comment: what the conflict was, why the chosen resolution was selected, and what alternatives were considered.
- **Tag stakeholders for semantic conflicts.** If a conflict involves business logic, tag the feature lead or product owner in the PR for review. Do not resolve business-rule conflicts unilaterally.
- **Document escalations.** If a conflict could not be resolved without external input, document: who was consulted, what decision was made, and the rationale. This lives in the resolution report.

#### 3.5 Special Considerations

##### Binary Files

- **Do not attempt to manually merge binary files** (images, compiled assets, `.dll`, `.pdb`, `.snk`). Accept one side and replace the other, or regenerate the binary from source after merging the source files.
- **For image assets**, accept the version from the branch that made the intentional change. If both branches intentionally changed the same image, flag for human review.

##### Configuration Files

- **Environment-specific config (`appsettings.Development.json`, `environment.ts`).** Preserve both sets of development settings unless they conflict on the same key, in which case the more complete or more recent value wins.
- **Shared config (`appsettings.json`, `angular.json`, `docker-compose.yml`).** Merge carefully. Conflicting values on the same key must be resolved by referencing the architecture and requirements. Run the application after resolution to verify.

##### Lock Files (`package-lock.json`, `*.csproj`, `packages.lock.json`)

- **Do not manually edit lock files.** Accept one side, then run the package manager's restore/install command (`npm install`, `dotnet restore`) to regenerate a correct lock file that reflects the merged dependency declarations.
- **If `package.json` and `package-lock.json` both conflict**, resolve `package.json` first, then regenerate the lock file.

### 4. Build the Consolidated Branch

After resolving all conflicts in all files:

1. **Build the entire solution.** Every project must compile without errors. Fix any compilation errors introduced by the merge — these are non-negotiable.
2. **Run the linter/formatter.** Apply the project's linting rules (`dotnet format`, `ng lint`, ESLint) to the entire codebase. Fix any violations introduced by the merge.
3. **Regenerate any auto-generated files.** If the merge touched source files that drive code generation (OpenAPI specs, migration sources, Protobuf definitions), regenerate the output files.

### 5. Post-Resolution Validation

#### 5.1 Automated Validation

Run every test suite in this order:

1. **Backend unit tests** — `dotnet test` on Domain, Application, and Infrastructure unit test projects.
2. **Backend integration tests** — `dotnet test` on Api and Infrastructure integration test projects.
3. **Frontend unit tests** — `ng test` or `jest`.
4. **End-to-end tests** — `ng e2e` or Playwright.

For each suite, capture: total tests, passed, failed, skipped, and duration. Compare against the pre-merge baseline from Section 2.1. **Any test that passed before the merge and fails after must be investigated and fixed before the merge is considered complete.**

#### 5.2 Manual Validation

- **Smoke test the feature.** Walk through the primary user journeys defined in `../01-requirements/output/feature-requirements.md`. Verify that the merged code correctly implements each acceptance criterion.
- **Visual review for UI changes.** If the merge touched Angular components or styles, visually inspect the affected pages in a running environment. Check for layout breakage, missing elements, or style regressions.
- **Database verification.** If the merge touched SQL migrations, run the merged migrations against a test database. Verify that all tables, columns, indexes, and constraints are present and correct.
- **API contract check.** If the merge touched API endpoints, verify that the OpenAPI/Swagger document is accurate and that the Angular frontend's HTTP service calls match the merged API surface.

#### 5.3 Regression Checks

- **Run the NFR validation suite.** Re-check the quantified thresholds from `../01-requirements/output/non-functional-requirements.md` — performance benchmarks, security scans, accessibility audits. A merge must not degrade NFR compliance.
- **Re-run any failing tests from Stage 04.** If the review stage identified test failures, verify that they are still addressed and not reintroduced by the merge.
- **Tenant isolation check.** Verify that tenant-scoped data from one tenant does not leak into another tenant's context after the merge.

### 6. Write the Resolution Report

Save the consolidated report to `.\output\resolve-conflicts-report.md`. Use the following skeleton:

```markdown
# Merge Conflict Resolution Report

**Feature:** [Feature name from requirements]
**Version:** [Version number]
**Date:** [YYYY-MM-DD]
**Resolver(s):** [Name(s) of person(s) who resolved conflicts]

---

## 1. Pre-Merge Baseline

| Metric                           | Target Branch (pre-merge) |
|----------------------------------|---------------------------|
| Build status                     | Pass / Fail               |
| Unit tests (passed / total)      | X / Y                     |
| Integration tests (passed/total) | X / Y                     |
| E2E tests (passed / total)       | X / Y                     |
| Lint violations                  | N                         |

---

## 2. Conflict Inventory

| # | File                           | Conflict Type   | Branch A Author | Branch B Author | Description                                   |
|---|--------------------------------|-----------------|-----------------|-----------------|-----------------------------------------------|
| 1 | src/Api/Controllers/X.cs       | Semantic        | @author1        | @author2        | Both modified the same POST endpoint logic    |
| 2 | src/App/features/y.component.ts | Textual        | @author3        | @author4        | Import ordering and whitespace                |

**Summary:**
- Total conflicts: N
- Textual: N | Additive: N | Semantic: N | Architectural: N | Data/Schema: N | Dependency: N | Generated: N

---

## 3. Resolution Log

### Conflict #1: [File path] — [Brief Description]

- **Type:** [Conflict type from taxonomy]
- **Authors:** @authorA (Branch A), @authorB (Branch B)
- **What each branch changed:**
  - Branch A: [Summary of change]
  - Branch B: [Summary of change]
- **Resolution:** [What the merged code looks like and why]
- **Rationale:** [Which standard, requirement, or ADR guided this resolution]
- **Escalation required:** Yes / No
  - If yes: Consulted [name/role], decision: [outcome]

### Conflict #2: [File path] — [Brief Description]

...

---

## 4. Post-Resolution Build & Test Results

| Metric                           | Pre-Merge Baseline | Post-Resolution | Delta     |
|----------------------------------|---------------------|-----------------|-----------|
| Build status                     |                     |                 |           |
| Unit tests (passed / total)      |                     |                 |           |
| Integration tests (passed/total) |                     |                 |           |
| E2E tests (passed / total)       |                     |                 |           |
| Lint violations                  |                     |                 |           |
| NFR compliance                   |                     |                 |           |

**Regression summary:** [Any tests that passed before but fail now, with investigation notes]

---

## 5. Manual Validation Checklist

- [ ] Smoke test of primary user journeys — all acceptance criteria met
- [ ] Visual review of UI components — no layout or style regressions
- [ ] Database migration test — schema matches data model from architecture
- [ ] API contract verification — OpenAPI doc matches implementation
- [ ] Tenant isolation check — no cross-tenant data leakage

---

## 6. Open Items & Follow-Ups

| # | Item                                        | Owner  | Due Date   | Status |
|---|---------------------------------------------|--------|------------|--------|
| 1 | [Any unresolved concern or technical debt]  | @owner | YYYY-MM-DD | Open   |

---

## 7. Sign-Off

- [ ] All conflicts resolved
- [ ] All tests pass (or known failures documented with rationale)
- [ ] PR reviewed by at least one other developer
- [ ] Feature lead notified of any semantic resolutions
```

---

## Summary Checklist

Before marking this stage complete, verify:

- [ ] All input documents from `../../docs/`, `../01-requirements/output/`, `../02-architecture/output/`, `../03-implementation/output/`, `../04-review-and-testing/output/`, and `./references/` have been loaded and reviewed.
- [ ] Every merge conflict across the entire codebase has been identified and classified.
- [ ] Every conflict has been resolved following the language-specific standards in Section 3.2.
- [ ] Architectural integrity has been verified: layer boundaries, SOLID principles, multi-tenancy, and API contracts are intact.
- [ ] Binary files, lock files, and generated code have been handled per Section 3.5.
- [ ] The full test suite passes with no regressions from the pre-merge baseline.
- [ ] Manual validation — smoke tests, visual review, database verification, API check, and tenant isolation check — is complete.
- [ ] All authors of conflicting code have been notified of the resolutions.
- [ ] Any escalations have been documented with decisions and rationale.
- [ ] The consolidated resolution report has been saved to `.\output\resolve-conflicts-report.md`.
- [ ] The report includes the conflict inventory, resolution log with rationale for each conflict, post-resolution test results, manual validation checklist, open items, and sign-off.
