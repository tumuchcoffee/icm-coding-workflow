#!/bin/bash
# source/02-backend/entrypoint.sh
# ADR-013: DbUp migration execution at API container entrypoint
# Uses bash /dev/tcp for SQL Server probing — no sqlcmd dependency needed.
set -e

echo "=== ICM API Container Entrypoint ==="
echo "Waiting for SQL Server to be ready..."

# Wait for SQL Server to accept TCP connections (max 120 seconds, aligned with NFR-001)
RETRIES=24
until bash -c "echo > /dev/tcp/icm-db/1433" 2>/dev/null; do
  RETRIES=$((RETRIES - 1))
  if [ $RETRIES -le 0 ]; then
    echo "ERROR: SQL Server did not become ready within 120 seconds."
    exit 1
  fi
  echo "SQL Server not ready yet. Retrying in 5 seconds... ($RETRIES retries left)"
  sleep 5
done

echo "SQL Server is ready. Running DbUp migrations..."

# Run DbUp migrations
# The --migrate-only flag is handled by the API's Program.cs which
# detects the flag and runs DbUp, then exits with code 0 if successful.
dotnet Api.dll --migrate-only

if [ $? -ne 0 ]; then
  echo "ERROR: Database migration failed."
  exit 1
fi

echo "Migrations complete. Starting API..."

# Start the API (or dotnet watch in dev mode)
if [ "$1" = "watch" ]; then
  echo "Starting in development mode (dotnet watch)..."
  exec dotnet watch run --project src/Api/Api.csproj --urls "http://+:5001"
else
  echo "Starting in production mode (Kestrel)..."
  exec dotnet Api.dll
fi