# source/02-backend/Dockerfile
# .NET 10 Web API — multi-stage Docker build
# ADR-011: Multi-stage builds for all custom images
# ADR-013: DbUp migrations at entrypoint
# ADR-017: Framework-dependent deployment on aspnet:10.0

# ── Stage 1: Build ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for layer caching
COPY source/02-backend/src/Api/Api.csproj Api/
COPY source/02-backend/src/Application/Application.csproj Application/
COPY source/02-backend/src/Domain/Domain.csproj Domain/
COPY source/02-backend/src/Infrastructure/Infrastructure.csproj Infrastructure/

# Restore dependencies
RUN dotnet restore "Api/Api.csproj"

# Copy remaining source and publish
COPY source/02-backend/src/ .
RUN dotnet publish "Api/Api.csproj" -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime (default target) ───────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
# ADR-017: Framework-dependent deployment on Ubuntu-based ASP.NET image

WORKDIR /app

# Install curl for health checks only.
# SQL Server probing uses bash /dev/tcp — no sqlcmd needed (saves ~130 MB).
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Copy SQL migrations for DbUp
COPY source/03-sql/migrations ./migrations/

# Copy and set entrypoint script
COPY source/02-backend/entrypoint.sh .
RUN chmod +x entrypoint.sh

EXPOSE 5001
ENTRYPOINT ["./entrypoint.sh"]

# ── Stage 3: Development (dotnet watch) — optional target ───────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development
# ADR-015: Dev profile for hot reload
WORKDIR /app

# Install curl for health checks only.
# SQL Server probing uses bash /dev/tcp — no sqlcmd needed (saves ~130 MB).
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

# Copy entrypoint for migrations
COPY source/02-backend/entrypoint.sh .
RUN chmod +x entrypoint.sh

# Copy SQL migrations for DbUp
COPY source/03-sql/migrations ./migrations/

# Source is mounted as a volume at runtime — not copied
# The entrypoint runs migrations, then starts dotnet watch
ENTRYPOINT ["./entrypoint.sh", "watch"]