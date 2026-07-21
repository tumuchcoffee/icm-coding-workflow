# Glossary — Docker Local Development

> **Generated:** 2026-07-20
> **Sources:** `references/requirements.md`, codebase inspection

---

| Term                  | Definition                                                                                              |
|-----------------------|---------------------------------------------------------------------------------------------------------|
| Synergistic           | The multi-tenant SaaS administration panel application (ICM Admin). Angular 19 UI + .NET 10 API + SQL Server. |
| Docker                | A containerization platform used to package the application and its dependencies into isolated units.    |
| Docker Compose        | A tool for defining and running multi-container Docker applications via a single YAML file.              |
| Dockerfile            | A text file containing instructions to build a Docker image for a single service.                       |
| Docker Network        | A virtual network that allows Docker containers to communicate with each other and (optionally) the host. |
| Image                 | A read-only template with instructions for creating a Docker container. Built from a Dockerfile.         |
| Container             | A runnable instance of a Docker image.                                                                  |
| Volume Mount          | A Docker feature that maps a host directory into a container, enabling live code editing without rebuild. |
| DbUp                  | A .NET library for deploying database changes via SQL migration scripts. Runs idempotently on startup.   |
| Migration             | A versioned SQL script that modifies the database schema. Stored in `source/03-sql/migrations/`.        |
| Health Check          | A Docker mechanism that periodically tests a container's readiness and reports its status.              |
| Hot Reload            | The ability to see code changes reflected in a running application without a full restart or rebuild.     |
| Multi-stage Build     | A Dockerfile pattern using multiple `FROM` statements to keep the final image small by separating build and runtime stages. |
| ICM                   | Incident Case Management — the domain for which Synergistic provides administration capabilities.         |
| SPA                   | Single Page Application — the Angular-based frontend (source/01-ui).                                    |
| .NET API              | The ASP.NET Core Web API backend (source/02-backend).                                                   |