# Review Action Items — v0.1.3 Docker Local Development

**Source**: `review-and-test-results.md`
**Date**: 2026-07-20

---

## 🔴 Blocking — Must Fix Before Merge

### 1. Add `user: root` to `icm-db` service

**File**: `docker-compose.yml`
**Issue**: SQL Server container fails to start on WSL2/Docker Desktop with named volumes — `BootstrapSystemDataDirectories() failure (HRESULT 0x80070005)`. The `mssql` user lacks permissions on the mounted volume directories.

**Fix**:
```yaml
icm-db:
  image: mcr.microsoft.com/mssql/server:2022-latest
  user: root   # <-- add this line
  ...
```

The SQL Server process still runs as `mssql` internally; only the container entrypoint needs root for volume permissions.

---

### 2. Fix API Dockerfile — Debian vs. Ubuntu repo mismatch

**File**: `source/02-backend/Dockerfile` (lines 31–37)
**Issue**: The runtime base image `aspnet:10.0` is Ubuntu 24.04 (Noble), but the Dockerfile uses a Debian 12 Microsoft package repo. Installing `packages-microsoft-prod.deb` for Debian 12 fails on Ubuntu.

**Fix**: Replace the Debian 12 repo with the Ubuntu 24.04 GPG-key approach:
```dockerfile
# Install mssql-tools18 on Ubuntu 24.04 (Noble)
RUN curl -sSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /usr/share/keyrings/microsoft-archive-keyring.gpg \
    && echo "deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft-archive-keyring.gpg] https://packages.microsoft.com/ubuntu/24.04/prod noble main" > /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y mssql-tools18 \
    && apt-get clean
```

---

### 3. Fix UI Dockerfile — Angular 19 output path

**File**: `source/01-ui/Dockerfile` (line 29)
**Issue**: `COPY --from=build /app/dist/icm-admin/browser` references a hardcoded project-name path. Angular 19 with `outputPath=dist` produces `dist/browser/`, not `dist/icm-admin/browser`.

**Fix**:
```dockerfile
COPY --from=build /app/dist/browser /usr/share/nginx/html
```

---

### 4. Fix API binding — remove loopback-only `Urls`

**File**: `source/02-backend/src/Api/appsettings.Development.json`
**Issue**: `"Urls": "http://localhost:5001"` binds only to loopback (127.0.0.1). This prevents the nginx proxy from reaching the API container over the internal Docker network.

**Fix**: Remove the `Urls` setting entirely. The `docker-compose.yml` already sets `ASPNETCORE_URLS=http://+:5001` which binds to all interfaces.
```json
// Remove this line:
"Urls": "http://localhost:5001"
```

---

### 5. Reduce API image size below 300 MB

**File**: `source/02-backend/Dockerfile`
**Issue**: API image is 388 MB — exceeds NFR-002 budget of 300 MB. `mssql-tools18` + ODBC drivers add ~130 MB.

**Options to consider**:
- Use an Alpine-based SDK stage and only include `mssql-tools18` in a slim runtime
- Move the migration step to a separate init container so the API image doesn't need sqlcmd at all
- Multi-stage: run migrations in the build stage, then copy only the result
- Trim self-contained publish: `dotnet publish -p:PublishTrimmed=true`

---

## 🟡 Warnings — Should Fix

### 6. Remove hardcoded SA password from connection string

**File**: `source/02-backend/src/Api/appsettings.Development.json`
**Issue**: SA password `SynergisticDev123!` is hardcoded in the connection string.

**Fix**: Reference the environment variable:
```json
"ConnectionStrings": {
  "Default": "Server=icm-db,1433;Database=Synergistic;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=true;Encrypt=false"
}
```

---

### 7. Conditionally expose port 1433

**File**: `docker-compose.yml`
**Issue**: SQL Server port 1433 is exposed to the host. This is a documented deviation for dev convenience, but should be conditional.

**Fix**: Consider moving the port mapping into the `dev` profile only, or adding a comment block that clearly marks it for removal in CI/production.

---

### 8. Increase SQL Server wait timeout for Apple Silicon

**File**: `source/02-backend/entrypoint.sh`
**Issue**: The wait loop retries 12× (60s max). On Apple Silicon cold starts, SQL Server may take longer.

**Fix**: Increase retries from 12 to 24 (120s) to align with NFR-001 cold-start threshold:
```bash
for i in $(seq 1 24); do
```

---

### 9. Create a minimal test project for `DatabaseMigrator`

**File**: `source/02-backend/Synergistic.sln`
**Issue**: No automated test projects exist — zero assertions for any layer.

**Fix**: At minimum, add a single xUnit test project with a unit test for `DatabaseMigrator`:
- Test that `PerformUpgrade()` returns `MigrationResult.Success()` when given a valid connection string
- Test that `PerformUpgrade()` returns `MigrationResult.Failure()` on an invalid connection string

---

## 🔵 Observations — Non-Blocking

### 10. Pin `dbup-sqlserver` version explicitly

**File**: `source/02-backend/src/Infrastructure/Infrastructure.csproj`
**Issue**: NU1603 warning — `dbup-sqlserver` 6.0.4 not found, resolved to 6.0.16. Pin to `6.0.16` explicitly to suppress restore warnings.

---

### 11. Angular bundle size warning

**File**: `source/01-ui`
**Issue**: Build warning — initial bundle (559.88 kB) exceeds budget (500 kB). Already noted by implementation team. Monitor and tree-shake as needed.

---

### 12. npm audit — 30 vulnerabilities

**File**: `source/01-ui`
**Issue**: 30 npm vulnerabilities (2 low, 10 moderate, 18 high). Run `npm audit fix` and re-test.

---

### 13. Migrate off deprecated `@primeng/themes`

**File**: `source/01-ui`
**Issue**: `@primeng/themes@21.0.4` is deprecated. Migrate to `@primeuix/themes`.

---

### 14. Add `.env` to `.gitignore`

**File**: `docker-compose.yml` / `.gitignore`
**Issue**: Ensure `.env` is in `.gitignore` to prevent the SA password from being committed. `.env.example` is safe to keep in source control.

---

## Summary

| Priority | Count | Description |
|----------|-------|-------------|
| 🔴 Critical | 5 | Blocking issues — 4 already fixed during review, 1 remaining (`user: root`) + image size |
| 🟡 Warning | 4 | Should fix before next feature branch |
| 🔵 Info | 5 | Non-blocking observations |
| **Total** | **14** | |