# Global Coding Standards

## Definitions
Prefer the standards and conventions found in the recommendations that are provided by the technology and language inventors unless overridden by the team's own adaptations.

## Technologies

### C#
- Follow the [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) and [Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/) from Microsoft.
- Use PascalCase for class names, method names, and public members; camelCase for parameters and local variables.
- Prefer explicit access modifiers — do not rely on defaults.
- Use `var` when the type is obvious from the right-hand side; use explicit types otherwise.
- Keep methods small and focused on a single responsibility (SRP).
- Prefer async/await over synchronous blocking calls for I/O-bound operations.
- Use expression-bodied members for simple properties and methods where it improves readability.
- Enable nullable reference types and address warnings appropriately.
- Organize usings inside the namespace declaration; remove unused usings.

### Angular / TypeScript
- Follow the [Angular Style Guide](https://angular.dev/style-guide) and [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/intro.html) conventions.
- Use strong typing — avoid `any` unless absolutely necessary; prefer `unknown` for truly unknown types.
- Use standalone components, signals, and the new control flow syntax (`@if`, `@for`) for Angular 17+ projects.
- Keep components small, focused, and named with the `.component.ts` suffix; limit template logic.
- Use `async` pipe for observable subscriptions in templates to avoid manual subscription management.
- Prefer `interface` over `type` for object shapes; use `type` for unions, intersections, and mapped types.
- Organize files by feature (feature-based folder structure) rather than by type.
- Use ESLint with the recommended TypeScript and Angular rulesets; run linting as a pre-commit hook.
- Prefer pure functions and immutable data patterns where practical.
- Write unit tests with Jasmine/Karma or Jest for business logic and component interactions.

### SQL Server
- Follow the [Transact-SQL Conventions (Microsoft)](https://learn.microsoft.com/en-us/sql/t-sql/language-reference) and team-agreed naming standards.
- Use PascalCase for table names, view names, and stored procedure names; avoid prefixes like `tbl_`, `sp_`, `vw_`.
- Use snake_case or camelCase for column names (team decision); be consistent across the database.
- Always specify the schema (e.g., `dbo.`) when referencing objects.
- Use parameterized queries for all data access — never concatenate user input into SQL strings.
- Include meaningful comments in stored procedures and complex queries explaining the intent.
- Prefer `VARCHAR`/`NVARCHAR` over `CHAR`/`NCHAR` unless fixed-length data is required.
- Use CTEs (Common Table Expressions) over subqueries for readability in complex queries.
- Index foreign key columns and columns used frequently in `WHERE`, `JOIN`, and `ORDER BY` clauses.
- Ensure all scripts are idempotent — check for object existence before creating or altering.
- Keep transactions short and avoid holding locks longer than necessary.

### Azure
- Follow the [Azure Well-Architected Framework](https://learn.microsoft.com/en-us/azure/well-architected/) pillars: reliability, security, cost optimization, operational excellence, and performance efficiency.
- Use [Azure Naming Conventions](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-naming) for consistent resource naming — include environment, region, and instance where applicable.
- Infrastructure as Code (IaC) is preferred: use Bicep or Terraform for provisioning; avoid manual portal-based resource creation in production.
- Store secrets, connection strings, and keys in Azure Key Vault — never check them into source control.
- Use Managed Identity for service-to-service authentication where supported; avoid connection strings with embedded credentials.
- Implement retry policies with exponential backoff using Polly or the built-in Azure SDK retry mechanisms.
- Use Azure Monitor, Application Insights, and Log Analytics for observability; configure alerts on meaningful thresholds.
- Tag all resources with at minimum `Environment`, `Owner`, `Project`, and `CostCenter` for governance and cost tracking.
- Follow the principle of least privilege for all RBAC assignments; prefer built-in roles over custom roles when possible.
- Use resource groups as lifecycle boundaries — group resources that share the same deploy, update, and delete cycle.
- Prefer serverless and PaaS options (Azure Functions, App Service, Azure SQL) over IaaS unless there is a specific need for VM-level control.