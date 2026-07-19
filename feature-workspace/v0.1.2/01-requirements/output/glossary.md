# Glossary — Code Initialization (v0.1.2)

**Date**: 2026-07-18
**Feature**: Code Initialization — project scaffolding, containers, and database setup

---

| Term                           | Definition                                                                                                                                                        |
| ------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Personal Dashboard**   | The multi-tenant SaaS administration panel application. The user-facing product name.                                                                             |
| **Angular SPA**          | The single-page application frontend built with Angular (latest LTS). Runs on`localhost:4200` in development.                                                   |
| **.NET API**             | The backend web API built with .NET 10. Runs on`localhost:5001` in development.                                                                                 |
| **Health Check**         | A lightweight, self-contained endpoint (`GET /api/health`) that returns the API's status, timestamp, and version. No database call in v0.1.2.                   |
| **PrimeNG**              | A UI component library for Angular providing pre-built, accessible controls (buttons, avatars, sidebars, menus).                                                  |
| **Standalone Component** | An Angular component that does not require an`NgModule`. The default and recommended style in Angular 19+.                                                      |
| **Slide-out Menu**       | A navigation panel that expands from the left side of the screen when the hamburger icon is clicked. Built with PrimeNG`p-sidebar`.                             |
| **Detail Pane**          | An optional, conditional right-hand panel (default 150px) that child feature components can toggle open for contextual information.                               |
| **Sticky Header/Footer** | UI elements that remain fixed at the top (header) or bottom (footer) of the viewport and do not scroll with the page content.                                     |
| **App Shell**            | The persistent UI chrome (Header, Menu, Content Area, optional Detail Pane, Footer) that surrounds routed feature components.                                     |
| **Router Outlet**        | The Angular Router placeholder (`<router-outlet>`) where feature components are rendered based on the active route.                                             |
| **Clean Architecture**   | A layered architecture pattern where dependencies point inward: Api → Application → Domain, with Infrastructure implementing interfaces defined by Application. |
| **LocalDB**              | A lightweight version of SQL Server Express designed for local development. Used as the database engine in v0.1.2.                                                |
| **SchemaVersion**        | A tracking table (`dbo.SchemaVersion`) that records which migration scripts have been applied to the database.                                                  |
| **Postman Collection**   | A saved set of HTTP requests in Postman format, located in`\source\04-testing\postman`, used to verify the API.                                                 |
| **MoSCoW**               | A prioritization framework: Must have, Should have, Could have, Won't have (this time).                                                                           |
| **GIVEN/WHEN/THEN**      | A Gherkin-style format for acceptance criteria: GIVEN a precondition, WHEN an action occurs, THEN an expected outcome.                                            |
