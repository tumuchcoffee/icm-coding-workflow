# Feature Requirements — Code Initialization (v0.1.2)

**Date**: 2026-07-18
**Feature**: Code Initialization — project scaffolding, containers, and database setup
**Tier**: Lightweight (score: 6 — solo developer, well-understood domain, low risk, no regulatory exposure, standalone, continuous delivery)
**Sources**: `../01-requirements/references/requirements.md`

---

## User Personas

### P1: Software Engineer (Primary)
**Role**: A developer contributing to the Synergistic codebase.
**Goal**: Run and develop the full application stack locally, verify the end-to-end pipeline works, and have a foundation to build features on.

---

## Functional Requirements

### FR-001: Local Development Environment — Run Full Stack
**As a** software engineer,
**I want** to run the entire application from my local system with a single command,
**so that** I can develop and test features without depending on shared infrastructure.

**Acceptance Criteria**:
- [ ] GIVEN I have cloned the repository and installed prerequisites (Node.js, .NET 10 SDK, SQL Server LocalDB)
      WHEN I run the startup script
      THEN the Angular SPA is available at http://localhost:4200
- [ ] GIVEN I have cloned the repository and installed prerequisites
      WHEN I run the startup script
      THEN the .NET API is available at http://localhost:5001
- [ ] GIVEN I have cloned the repository and installed prerequisites
      WHEN I run the startup script
      THEN the SQL Server `Synergistic` database is created and accessible from the API

**Sources:** requirements.md (Angular, .NET API, Database sections)

---

### FR-002: Angular Shell Application — Header Component
**As a** software engineer,
**I want** a sticky header that displays the application title and navigation controls,
**so that** users can identify the application and access the main menu.

**Acceptance Criteria**:
- [ ] GIVEN the Angular application is loaded
      WHEN the page renders
      THEN a sticky header is displayed at the top of the viewport
- [ ] GIVEN the header is rendered
      WHEN I inspect the header
      THEN it contains a hamburger icon (left), an H1 tag with text "Synergistic" (left), and a user profile icon (far right)
- [ ] GIVEN the header is rendered
      WHEN I scroll the page
      THEN the header remains fixed at the top and does not scroll with the content
- [ ] GIVEN the user profile icon is rendered
      WHEN I click the user profile icon
      THEN it navigates to the root URL (`/`)

**Sources:** requirements.md (Angular section — Header)

---

### FR-003: Drop-down Menu (Slide-out Panel)
**As a** software engineer,
**I want** a slide-out navigation menu that expands from the left of the screen,
**so that** users can navigate between different sections of the application.

**Acceptance Criteria**:
- [ ] GIVEN the application is loaded and the menu is closed
      WHEN I click the hamburger icon in the header
      THEN a navigation panel slides out from the left side of the screen
- [ ] GIVEN the menu panel is open
      WHEN I inspect the panel contents
      THEN each navigation link is rendered as an H3 tag
- [ ] GIVEN the menu panel is open
      WHEN I click the close icon or click outside the panel
      THEN the panel slides closed

**Sources:** requirements.md (Angular section — Drop-down menu)

---

### FR-004: Footer Component
**As a** software engineer,
**I want** a sticky footer that displays the application name,
**so that** the application has consistent branding at the bottom of every page.

**Acceptance Criteria**:
- [ ] GIVEN the Angular application is loaded
      WHEN the page renders
      THEN a sticky footer is displayed at the bottom of the viewport
- [ ] GIVEN the footer is rendered
      WHEN I inspect the footer
      THEN it contains a centered H3 tag with the text "Synergistic"
- [ ] GIVEN the footer is rendered
      WHEN I scroll the page
      THEN the footer remains fixed at the bottom and does not scroll with the content

**Sources:** requirements.md (Angular section — Footer)

---

### FR-005: Optional Detail Pane (Right-Hand Panel)
**As a** software engineer,
**I want** an optional right-hand detail pane that can be toggled by child components,
**so that** future features can display contextual information without navigating away.

**Acceptance Criteria**:
- [ ] GIVEN a feature component requests the detail pane
      WHEN the pane is toggled open
      THEN a panel expands from the right side with a default width of 150px
- [ ] GIVEN the detail pane is open
      WHEN the pane is toggled closed
      THEN the panel collapses and the content area reclaims the space
- [ ] GIVEN the detail pane is not needed by any feature
      WHEN the application renders
      THEN the detail pane is not visible and does not occupy any space

**Sources:** requirements.md (Angular section — Optional right-hand detail pane)

---

### FR-006: Content Area (Router Outlet)
**As a** software engineer,
**I want** a main content area that hosts feature components via Angular Router,
**so that** the application supports navigation between different views.

**Acceptance Criteria**:
- [ ] GIVEN the application shell is rendered
      WHEN a route is activated
      THEN the corresponding component is rendered inside the content area between the header and footer
- [ ] GIVEN no route is active
      WHEN the application loads
      THEN the content area is empty and ready to host routed components

**Sources:** requirements.md (Angular section — Content area)

---

### FR-007: .NET API — Health Check Endpoint
**As a** software engineer,
**I want** a health check endpoint in the .NET API,
**so that** I can verify the API is running and the full-stack pipeline (Angular → .NET → SQL Server) is functional.

**Acceptance Criteria**:
- [ ] GIVEN the .NET API is running
      WHEN I send a GET request to `/api/health`
      THEN the API returns HTTP 200 OK
- [ ] GIVEN the health endpoint is called
      WHEN the response is returned
      THEN it includes a JSON body with `status` (string), `timestamp` (ISO 8601 UTC), and `version` (string matching "0.1.2")
- [ ] GIVEN the API is not running
      WHEN I send a request to `/api/health`
      THEN the request fails with a connection error

**Sources:** requirements.md (.NET API section)

---

### FR-008: SQL Server Database — Schema Initialization
**As a** software engineer,
**I want** a scripted SQL Server database that can be created on a local development machine,
**so that** the database layer is ready for future schema migrations.

**Acceptance Criteria**:
- [ ] GIVEN SQL Server LocalDB is installed
      WHEN I run the database creation script
      THEN a database named `Synergistic` is created
- [ ] GIVEN the database exists
      WHEN I inspect the schema
      THEN a `dbo.SchemaVersion` table exists to track applied migrations
- [ ] GIVEN the database creation is complete
      WHEN I check the `\source\03-sql\script` folder
      THEN a scripted copy of all database objects is present for AI model access

**Sources:** requirements.md (Database section)

---

### FR-009: No Authentication in v0.1.2
**As a** software engineer,
**I want** the application to be accessible without authentication in this version,
**so that** I can validate the full-stack pipeline without configuring an identity provider.

**Acceptance Criteria**:
- [ ] GIVEN the Angular application is running
      WHEN I access any route
      THEN no login redirect or auth guard blocks the request
- [ ] GIVEN the .NET API is running
      WHEN I call any endpoint
      THEN no 401 Unauthorized response is returned due to missing auth tokens
- [ ] GIVEN the user profile icon in the header
      WHEN clicked
      THEN it navigates to `/` (placeholder behavior until authentication is implemented)

**Sources:** requirements.md (Angular section — "This version will not have authentication")

---

### FR-010: Postman Test Collection
**As a** software engineer,
**I want** a Postman collection that calls the health check endpoint,
**so that** I can verify the API is working without manually constructing HTTP requests.

**Acceptance Criteria**:
- [ ] GIVEN the Postman collection is imported
      WHEN I run the health check request
      THEN the request targets `GET {{baseUrl}}/api/health`
- [ ] GIVEN the health check request is executed
      WHEN the response is received
      THEN automated test assertions verify HTTP 200 and the presence of `status`, `timestamp`, and `version` fields
- [ ] GIVEN the collection exists
      WHEN I check the `\source\04-testing\postman` folder
      THEN the collection file is present and importable into Postman

**Sources:** requirements.md (Testing section)

---

### FR-011: Angular — PrimeNG Component Library
**As a** software engineer,
**I want** PrimeNG components as the default UI control library,
**so that** I can rapidly build the UI shell with pre-built, accessible components.

**Acceptance Criteria**:
- [ ] GIVEN the Angular project is initialized
      WHEN I check the dependencies
      THEN PrimeNG (version compatible with Angular 19+) is installed
- [ ] GIVEN PrimeNG is installed
      WHEN I inspect the header component template
      THEN the hamburger button uses a PrimeNG `p-button` and the user icon uses a PrimeNG `p-avatar`
- [ ] GIVEN PrimeNG is installed
      WHEN I inspect the slide-out menu
      THEN the panel uses a PrimeNG overlay component (e.g., `p-sidebar`)

**Sources:** requirements.md (Angular section — "use the matching version of PrimeNG components as the default choice for controls")

---

### FR-012: Angular — Latest LTS Version
**As a** software engineer,
**I want** the Angular project created with the latest LTS version as of 2026-07-18,
**so that** the codebase starts on a supported, long-term foundation.

**Acceptance Criteria**:
- [ ] GIVEN the Angular project is initialized
      WHEN I check the Angular version in `package.json`
      THEN the version is the latest LTS release available on 2026-07-18
- [ ] GIVEN the project is scaffolded
      WHEN I inspect the project structure
      THEN it uses standalone components (no NgModules) and the new control flow syntax (`@if`, `@for`)

**Sources:** requirements.md (Angular section — "Create the project for the angular code using the latest LTS version as of today's date 2026-07-18")

---

## Gaps and Contradictions Flagged

| # | Issue | Severity | Recommendation |
|---|-------|----------|----------------|
| G-001 | No explicit NFRs stated in the source requirements. Performance, accessibility, and security thresholds are absent. | Medium | Apply default web app NFRs: page load < 3s, WCAG 2.1 AA, HTTPS only. Quantify in the NFR document. |
| G-002 | "Latest LTS version" is a moving target — it will change over time. | Low | Pin the exact version in the implementation and ADR. |
| G-003 | No mention of browser support requirements. | Low | Default to latest 2 versions of Chrome, Edge, Firefox, and Safari. |
| G-004 | No mention of the framework for .NET (Minimal API vs. Controllers). The requirements say "Health Check controller" but the system architecture specifies Minimal APIs. | Low | Resolve during architecture stage — the architecture CONTEXT takes precedence. |