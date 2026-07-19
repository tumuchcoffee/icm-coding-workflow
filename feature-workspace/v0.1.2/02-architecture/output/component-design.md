# Component Design — Code Initialization (v0.1.2)

**Feature**: Code Initialization — project scaffolding, containers, and database setup
**Date**: 2026-07-18
**Version**: v0.1.2

---

## 1. Angular Frontend Components

### 1.1 AppShellComponent

| Attribute | Detail |
|---|---|
| **Responsibility** | Hosts the full-screen layout grid: header (top), menu overlay, content area (center), optional detail pane (right), footer (bottom). Provides the `<router-outlet>` where feature components render. |
| **Files** | `src/app/layout/app-shell/app-shell.component.ts`<br/>`src/app/layout/app-shell/app-shell.component.html`<br/>`src/app/layout/app-shell/app-shell.component.scss` |
| **Selector** | `<app-shell>` |
| **Dependencies** | `HeaderComponent`, `MenuComponent`, `FooterComponent`, `DetailPaneComponent`, `RouterOutlet` (Angular Router) |
| **Interface** | No inputs or outputs. Self-contained layout. |
| **Error Handling** | None — this is a pure layout container. Router handles 404 for unknown routes. |
| **Tenant Awareness** | None in v0.1.2 (ADR-005) |
| **Traceability** | FR-001, FR-002, FR-003, FR-004, FR-005, FR-006 |
| **Principle** | **Single Responsibility** — the shell only handles layout; it delegates every UI region to a dedicated component. **Separation of Concerns** — TypeScript logic in `.ts`, HTML structure in `.html`, and SCSS styling in `.scss` (ADR-008). |

**Template Structure** (from `app-shell.component.html`):
```html
<app-header (menuToggle)="toggleMenu()" />
<app-menu [isOpen]="menuOpen()" (closed)="closeMenu()" />

<div class="content-area">
  <router-outlet />
  <app-detail-pane [isOpen]="detailPaneOpen()">
    <!-- Feature content projected here -->
  </app-detail-pane>
</div>

<app-footer />
```

**CSS Layout** (from `app-shell.component.scss`): CSS Grid with three rows: `auto` (header), `1fr` (content + optional pane), `auto` (footer). The menu is a positioned overlay, not part of the grid.

**Component API** (from `app-shell.component.ts`):
```typescript
@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    RouterOutlet,
    HeaderComponent,
    MenuComponent,
    FooterComponent,
    DetailPaneComponent
  ],
  templateUrl: './app-shell.component.html',
  styleUrls: ['./app-shell.component.scss']
})
export class AppShellComponent {
  menuOpen = signal<boolean>(false);
  detailPaneOpen = signal<boolean>(false);

  toggleMenu(): void {
    this.menuOpen.update(v => !v);
  }

  closeMenu(): void {
    this.menuOpen.set(false);
  }
}
```

---

### 1.2 HeaderComponent

| Attribute | Detail |
|---|---|
| **Responsibility** | Renders the sticky application header with hamburger menu button, application title, and user profile avatar. |
| **Files** | `src/app/layout/header/header.component.ts`<br/>`src/app/layout/header/header.component.html`<br/>`src/app/layout/header/header.component.scss` |
| **Selector** | `<app-header>` |
| **Dependencies** | PrimeNG `p-button` (icon-only, text variant), PrimeNG `p-avatar` (icon variant with `pi pi-user`), Angular Router |
| **Interface** | **Output**: `menuToggle: OutputEmitterRef<void>` — emitted when the hamburger button is clicked |
| **Error Handling** | None — purely presentational. The user icon navigates to `/` (placeholder for future user profile route). |
| **Tenant Awareness** | None in v0.1.2 (ADR-005) |
| **Traceability** | FR-002 |
| **Principle** | **Single Responsibility** — the header only renders header chrome; it does not manage menu state or routing logic. **Separation of Concerns** — TypeScript, HTML, and SCSS in separate files per ADR-008. |

**Component API** (from `header.component.ts`):
```typescript
@Component({
  selector: 'app-header',
  standalone: true,
  imports: [Button, Avatar, RouterLink],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent {
  menuToggle = output<void>();
}
```

**Template** (from `header.component.html`):
```html
<header class="app-header">
  <div class="header-left">
    <p-button icon="pi pi-bars"
              styleClass="p-button-text p-button-rounded"
              (onClick)="menuToggle.emit()" />
    <h1 class="header-title">FAST Dashboard</h1>
  </div>
  <a routerLink="/" class="header-profile">
    <p-avatar icon="pi pi-user" styleClass="cursor-pointer" />
  </a>
</header>
```

**Styling** (from `header.component.scss`): Sticky positioning with `position: sticky; top: 0; z-index: 40`. Height set to 64px. Flexbox layout with `justify-content: space-between` for left/right alignment. The left section holds the hamburger and title in a row; the right section holds the profile avatar. PrimeNG's built-in button and avatar styles provide the control-level styling.

---

### 1.3 MenuComponent

| Attribute | Detail |
|---|---|
| **Responsibility** | Renders a slide-out navigation panel from the left side of the screen. Contains navigation links as H3 elements. Managed open/closed via PrimeNG `p-sidebar`. |
| **Files** | `src/app/layout/menu/menu.component.ts`<br/>`src/app/layout/menu/menu.component.html`<br/>`src/app/layout/menu/menu.component.scss` |
| **Selector** | `<app-menu>` |
| **Dependencies** | PrimeNG `p-sidebar` |
| **Interface** | **Input**: `isOpen: InputSignal<boolean>` — controls panel visibility. **Output**: `closed: OutputEmitterRef<void>` — emitted when sidebar closes (close icon or click outside). |
| **Error Handling** | None. No dynamic content to fail rendering. |
| **Tenant Awareness** | None in v0.1.2 (ADR-005) |
| **Traceability** | FR-003 |
| **Principle** | **Single Responsibility** — the menu only handles navigation link rendering and open/close state; it delegates the overlay behavior to PrimeNG. **Separation of Concerns** — TypeScript, HTML, and SCSS in separate files per ADR-008. |

**Component API** (from `menu.component.ts`):
```typescript
@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [Sidebar],
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.scss']
})
export class MenuComponent {
  isOpen = input<boolean>(false);
  closed = output<void>();
}
```

**Template** (from `menu.component.html`):
```html
<p-sidebar [(visible)]="isOpen"
           position="left"
           styleClass="app-sidebar"
           (onHide)="closed.emit()">
  <ng-template pTemplate="content">
    <nav class="menu-nav">
      <!-- Navigation links added in future versions -->
      <h3 class="menu-link">Dashboard</h3>
    </nav>
  </ng-template>
</p-sidebar>
```

**Styling** (from `menu.component.scss`): Sidebar width 256px (`w-16rem` equivalent via SCSS). Navigation links use H3 tags per FR-003. Styling is handled via SCSS with PrimeNG's theme design tokens. Currently contains a single placeholder "Dashboard" link — actual feature routes added in future versions.

---

### 1.4 FooterComponent

| Attribute | Detail |
|---|---|
| **Responsibility** | Renders the sticky application footer with centered "FAST Dashboard" branding text. |
| **Files** | `src/app/layout/footer/footer.component.ts`<br/>`src/app/layout/footer/footer.component.html`<br/>`src/app/layout/footer/footer.component.scss` |
| **Selector** | `<app-footer>` |
| **Dependencies** | None (pure HTML/CSS) |
| **Interface** | No inputs or outputs. Static presentational component. |
| **Error Handling** | None. Static content cannot fail. |
| **Tenant Awareness** | None in v0.1.2 (ADR-005) |
| **Traceability** | FR-004 |
| **Principle** | **Single Responsibility** — the footer only renders footer chrome. **Separation of Concerns** — TypeScript, HTML, and SCSS in separate files per ADR-008. |

**Component API** (from `footer.component.ts`):
```typescript
@Component({
  selector: 'app-footer',
  standalone: true,
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.scss']
})
export class FooterComponent {}
```

**Template** (from `footer.component.html`):
```html
<footer class="app-footer">
  <h3 class="footer-text">FAST Dashboard</h3>
</footer>
```

**Styling** (from `footer.component.scss`): `position: sticky; bottom: 0` keeps it fixed at the viewport bottom. Height 48px. Text centered both horizontally and vertically using flexbox. Background color and border defined via PrimeNG theme design tokens.

---

### 1.5 DetailPaneComponent

| Attribute | Detail |
|---|---|
| **Responsibility** | Renders an optional, toggleable right-hand panel for contextual information. Content is projected from the parent/feature component. |
| **Files** | `src/app/layout/detail-pane/detail-pane.component.ts`<br/>`src/app/layout/detail-pane/detail-pane.component.html`<br/>`src/app/layout/detail-pane/detail-pane.component.scss` |
| **Selector** | `<app-detail-pane>` |
| **Dependencies** | None (Angular CDK optional for animation) |
| **Interface** | **Input**: `isOpen: InputSignal<boolean>` — controls visibility. Default width 150px. Content projected via `<ng-content>`. |
| **Error Handling** | None — content projection failures are handled by Angular's template compiler at build time. |
| **Tenant Awareness** | None in v0.1.2 (ADR-005) |
| **Traceability** | FR-005 |
| **Principle** | **Open/Closed** — the pane is open for extension (any feature can project content into it) but closed for modification (the panel itself never changes). **Separation of Concerns** — TypeScript, HTML, and SCSS in separate files per ADR-008. |

**Component API** (from `detail-pane.component.ts`):
```typescript
@Component({
  selector: 'app-detail-pane',
  standalone: true,
  templateUrl: './detail-pane.component.html',
  styleUrls: ['./detail-pane.component.scss']
})
export class DetailPaneComponent {
  isOpen = input<boolean>(false);
}
```

**Template** (from `detail-pane.component.html`):
```html
@if (isOpen()) {
  <aside class="detail-pane">
    <ng-content />
  </aside>
}
```

**Styling** (from `detail-pane.component.scss`): Fixed width 150px per FR-005, defined as a SCSS variable or PrimeNG spacing token. When closed, the element is removed from the DOM (`@if`) so it occupies zero space. Light gray background (`var(--surface-ground)` or equivalent PrimeNG theme variable) to visually distinguish from the content area.

---

## 2. .NET Backend Components

### 2.1 HealthCheckEndpoints

| Attribute | Detail |
|---|---|
| **Responsibility** | Maps the `GET /api/health` endpoint. Returns API status, current UTC timestamp, and version string. No database call, no authentication, no dependencies. |
| **File** | `src/Api/Endpoints/Health/HealthEndpoints.cs` |
| **Dependencies** | None. This is a self-contained endpoint with zero external calls. |
| **Interface** | **HTTP GET** `/api/health` → **Response 200**: `{ "status": "Healthy", "timestamp": "2026-07-18T...", "version": "0.1.2" }` |
| **Error Handling** | The endpoint cannot fail under normal conditions (no I/O). If the application is unreachable, the HTTP client handles the connection error. |
| **Tenant Awareness** | None in v0.1.2 (ADR-005). No `X-Tenant-Id` header required. |
| **Traceability** | FR-007 |
| **Principle** | **Single Responsibility** — the endpoint does exactly one thing: report liveness. **Separation of Concerns** — the endpoint mapping is in Api; the response model is in Application. |

**Code Design**:
```csharp
// Api/Endpoints/Health/HealthEndpoints.cs
public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/health")
            .WithTags("Health");

        group.MapGet("/", () =>
        {
            var response = new HealthCheckResponse(
                Status: "Healthy",
                Timestamp: DateTimeOffset.UtcNow.ToString("o"),
                Version: "0.1.2"
            );
            return Results.Ok(response);
        })
        .Produces<HealthCheckResponse>(StatusCodes.Status200OK)
        .WithName("GetHealth")
        .WithDescription("Returns the API health status, timestamp, and version.");
    }
}
```

```csharp
// Application/Features/Health/HealthCheckResponse.cs
namespace Synergistic.Application.Features.Health;

public sealed record HealthCheckResponse(
    string Status,
    string Timestamp,
    string Version
);
```

---

### 2.2 Api Layer — Program.cs

| Attribute | Detail |
|---|---|
| **Responsibility** | Bootstraps the ASP.NET Core application: registers services, configures middleware pipeline, maps endpoints. |
| **File** | `src/Api/Program.cs` |
| **Dependencies** | `Application`, `Infrastructure` (project references for DI registration) |
| **Interface** | N/A — application entry point |
| **Error Handling** | Startup exceptions (missing config, port conflicts) bubble up and terminate the process. The developer sees the error on the console. No global exception handler needed in v0.1.2 (no business logic to throw). |
| **Tenant Awareness** | None in v0.1.2. No tenant resolution middleware. |
| **Traceability** | FR-001, FR-007 |
| **Principle** | **Separation of Concerns** — Program.cs wires infrastructure; it does not contain business logic. |

**Code Design**:
```csharp
// Api/Program.cs
using Synergistic.Application;
using Synergistic.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapHealthEndpoints();
app.Run();
```

---

### 2.3 Application Layer — DependencyInjection

| Attribute | Detail |
|---|---|
| **Responsibility** | Registers Application-layer services. Currently a placeholder — no services to register. |
| **File** | `src/Application/DependencyInjection.cs` |
| **Dependencies** | `Domain` (project reference) |
| **Traceability** | ADR-001 |
| **Principle** | **Dependency Inversion** — Application depends on Domain, not the other way around. |

---

### 2.4 Domain Layer — Placeholder

| Attribute | Detail |
|---|---|
| **Responsibility** | Holds domain entities, value objects, and domain exceptions. Empty placeholder in v0.1.2 (no business logic). |
| **File** | `src/Domain/` (project with zero initial classes) |
| **Dependencies** | None. Domain has zero external dependencies per Clean Architecture. |
| **Traceability** | ADR-001 |
| **Principle** | **Clean Architecture** — Domain is the innermost layer. It owns the business rules. |

---

### 2.5 Infrastructure Layer — Placeholder

| Attribute | Detail |
|---|---|
| **Responsibility** | Implements interfaces defined by the Application layer (database access, external services). Empty placeholder in v0.1.2 (no database access). |
| **File** | `src/Infrastructure/DependencyInjection.cs` |
| **Dependencies** | `Application` (project reference, implements its interfaces) |
| **Traceability** | ADR-001 |
| **Principle** | **Dependency Inversion** — Infrastructure depends on Application abstractions, not the other way around. |

---

## 3. Database Components

### 3.1 SchemaVersion Table

| Attribute | Detail |
|---|---|
| **Responsibility** | Tracks which migration scripts have been applied to the database. A single row per migration. |
| **Database** | `Synergistic` (SQL Server LocalDB) |
| **Schema** | `dbo` |
| **Dependencies** | None |
| **Interface** | Created by DbUp migration `001_CreateSchemaVersion.sql`. Read by DbUp to determine pending migrations. |
| **Error Handling** | DbUp handles script application idempotently — if the table already exists, the script is skipped. |
| **Tenant Awareness** | None (ADR-005). `SchemaVersion` is a system table, not a tenant entity. |
| **Traceability** | FR-008 |
| **Principle** | **Idempotency** — migration scripts check for existence before creating. **Infrastructure as Code** — the schema is defined in versioned scripts, not manual GUI operations. |

**Column Definitions**:

| Column | Type | Nullable | Constraint | Description |
|---|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY(1,1) PRIMARY KEY CLUSTERED` | Auto-incrementing primary key |
| `ScriptName` | `nvarchar(255)` | NOT NULL | `UNIQUE` | Name of the migration script (e.g., `001_CreateSchemaVersion.sql`) |
| `Applied` | `datetime2(7)` | NOT NULL | `DEFAULT SYSUTCDATETIME()` | UTC timestamp when the migration was applied |

---

## 4. Startup Script

### 4.1 run.ps1

| Attribute | Detail |
|---|---|
| **Responsibility** | Single-command startup of the entire local development stack: apply database migrations, start .NET API, start Angular dev server. |
| **File** | `run.ps1` (repository root) |
| **Dependencies** | Node.js, .NET 10 SDK, SQL Server LocalDB |
| **Interface** | No arguments required. Runs three processes, outputs URLs to console. |
| **Error Handling** | Checks prerequisites before starting. Reports missing dependencies with installation links. Does not silently fail. |
| **Tenant Awareness** | N/A — infrastructure script |
| **Traceability** | FR-001 |
| **Principle** | **Simple over Complex** — one script, three tiers, zero configuration. |

**Script Flow**:
1. Verify prerequisites: `dotnet --version`, `node --version`, `sqllocaldb info`
2. Apply database migrations: `dotnet run --project src/Api -- --migrate` (or direct DbUp invocation)
3. Start .NET API: `dotnet run --project src/Api` (background process on `http://localhost:5001`)
4. Start Angular dev server: `npm start --prefix src/01-ui` (foreground on `http://localhost:4200`)

---

## 5. Postman Collection

### 5.1 icm-admin-v0.1.2.postman_collection.json

| Attribute | Detail |
|---|---|
| **Responsibility** | Provides a ready-to-run HTTP request that verifies the API health check endpoint. |
| **File** | `source/04-testing/postman/icm-admin-v0.1.2.postman_collection.json` |
| **Dependencies** | Postman (or Newman for CLI) |
| **Interface** | Collection variable `baseUrl` defaults to `http://localhost:5001`. Single request `GET {{baseUrl}}/api/health` with test assertions. |
| **Error Handling** | Postman test assertions fail with descriptive messages if the response is not HTTP 200 or required fields are missing. |
| **Tenant Awareness** | None (ADR-005) |
| **Traceability** | FR-010 |
| **Principle** | **Separation of Concerns** — testing artifacts are stored in the testing folder, not in the source tree. |

**Test Scripts** (Postman Test tab):
```javascript
pm.test("Status code is 200", () => {
    pm.response.to.have.status(200);
});

pm.test("Response has required fields", () => {
    const json = pm.response.json();
    pm.expect(json.status).to.be.a('string');
    pm.expect(json.timestamp).to.be.a('string');
    pm.expect(json.version).to.eql('0.1.2');
});
```

---

## 6. Solution Structure (v0.1.2)

```
icm-admin/
├── run.ps1                                    # Startup script
├── Synergistic.sln                               # .NET solution file
├── src/
│   ├── 01-ui/                                 # Angular SPA
│   │   ├── src/
│   │   │   └── app/
│   │   │       ├── layout/
│   │   │       │   ├── app-shell/
│   │   │       │   │   ├── app-shell.component.ts
│   │   │       │   │   ├── app-shell.component.html
│   │   │       │   │   └── app-shell.component.scss
│   │   │       │   ├── header/
│   │   │       │   │   ├── header.component.ts
│   │   │       │   │   ├── header.component.html
│   │   │       │   │   └── header.component.scss
│   │   │       │   ├── menu/
│   │   │       │   │   ├── menu.component.ts
│   │   │       │   │   ├── menu.component.html
│   │   │       │   │   └── menu.component.scss
│   │   │       │   ├── footer/
│   │   │       │   │   ├── footer.component.ts
│   │   │       │   │   ├── footer.component.html
│   │   │       │   │   └── footer.component.scss
│   │   │       │   └── detail-pane/
│   │   │       │       ├── detail-pane.component.ts
│   │   │       │       ├── detail-pane.component.html
│   │   │       │       └── detail-pane.component.scss
│   │   │       ├── app.config.ts
│   │   │       └── app.routes.ts
│   │   ├── package.json
│   │   ├── angular.json
│   ├── Api/
│   │   ├── Endpoints/
│   │   │   └── Health/
│   │   │       └── HealthEndpoints.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── Program.cs
│   ├── Application/
│   │   ├── Features/
│   │   │   └── Health/
│   │   │       └── HealthCheckResponse.cs
│   │   └── DependencyInjection.cs
│   ├── Domain/
│   │   └── Domain.csproj          # Empty — placeholder for entities
│   └── Infrastructure/
│       └── DependencyInjection.cs  # Placeholder DI registration
├── source/
│   ├── 03-sql/
│   │   ├── migrations/
│   │   │   └── 001_CreateSchemaVersion.sql
│   │   └── script/
│   │       └── (scripted copy of all DB objects)
│   └── 04-testing/
│       └── postman/
│           └── icm-admin-v0.1.2.postman_collection.json
└── docs/
    ├── coding-standards.md
    └── system-architecture.md
```

---

## 7. Component Interaction Summary

| Source Component | Target Component | Interaction | Via |
|---|---|---|---|
| `AppShellComponent` | `HeaderComponent` | Receives `menuToggle` event | `@Output()` binding |
| `AppShellComponent` | `MenuComponent` | Binds `isOpen` state, receives `closed` event | `@Input()` / `@Output()` binding |
| `AppShellComponent` | `DetailPaneComponent` | Binds `isOpen` state | `@Input()` binding |
| `HeaderComponent` | Angular Router | User icon navigates to `/` | `routerLink` |
| `Angular (browser)` | .NET API | `GET /api/health` | HTTP (dev: `http://localhost:5001`) |
| `run.ps1` | SQL Server LocalDB | Apply migrations | DbUp |
| `run.ps1` | .NET API | Start process | `dotnet run` |
| `run.ps1` | Angular | Start dev server | `npm start` |
| Postman | .NET API | `GET /api/health` | HTTP (dev: `http://localhost:5001`) |