<#
.SYNOPSIS
    Starts the full FAST Dashboard development stack: database migrations, .NET API, and Angular SPA.

.DESCRIPTION
    Phase 1: Applies pending DbUp migrations to the Synergistic database (SQL Server LocalDB).
    Phase 2: Starts the .NET 10 Web API on http://localhost:5001 (background process).
    Phase 3: Starts the Angular dev server on http://localhost:4200 (foreground process).

    Prerequisites: .NET 10 SDK, Node.js (LTS), SQL Server LocalDB

.EXAMPLE
    ./run.ps1
    Runs all three phases. Ctrl+C stops both the Angular and API processes.
#>

param(
    [switch]$SkipMigrations,
    [switch]$ApiOnly,
    [switch]$UiOnly
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# ─── Color helpers ───────────────────────────────────────────
function Write-Step { param([string]$Message) Write-Host "`n▶ $Message" -ForegroundColor Cyan }
function Write-OK { param([string]$Message) Write-Host "  ✅ $Message" -ForegroundColor Green }
function Write-Err { param([string]$Message) Write-Host "  ❌ $Message" -ForegroundColor Red }
function Write-Info { param([string]$Message) Write-Host "  ℹ️  $Message" -ForegroundColor Gray }

# ─── Header ──────────────────────────────────────────────────
Write-Host @"

╔═══════════════════════════════════════════════╗
║        FAST Dashboard — Startup Script        ║
║               Version 0.1.2                   ║
╚═══════════════════════════════════════════════╝

"@ -ForegroundColor Magenta

# ─── Prerequisites check ─────────────────────────────────────
Write-Step "Checking prerequisites..."

$allPrereqs = $true

# Check .NET 10 SDK
$dotnetVersion = $null
try {
    $dotnetOutput = & dotnet --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        $dotnetVersion = $dotnetOutput.Trim()
        if ($dotnetVersion.StartsWith("10.")) {
            Write-OK ".NET SDK $dotnetVersion"
        } else {
            Write-Err ".NET SDK $dotnetVersion found, but .NET 10 is required"
            Write-Info "Download .NET 10: https://dotnet.microsoft.com/en-us/download/dotnet/10.0"
            $allPrereqs = $false
        }
    } else {
        Write-Err ".NET SDK not found"
        Write-Info "Download .NET 10: https://dotnet.microsoft.com/en-us/download/dotnet/10.0"
        $allPrereqs = $false
    }
} catch {
    Write-Err ".NET SDK not found"
    Write-Info "Download .NET 10: https://dotnet.microsoft.com/en-us/download/dotnet/10.0"
    $allPrereqs = $false
}

# Check Node.js
try {
    $nodeVersion = & node --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-OK "Node.js $nodeVersion"
    } else {
        Write-Err "Node.js not found"
        Write-Info "Download Node.js LTS: https://nodejs.org/"
        $allPrereqs = $false
    }
} catch {
    Write-Err "Node.js not found"
    Write-Info "Download Node.js LTS: https://nodejs.org/"
    $allPrereqs = $false
}

# Check SQL Server LocalDB
try {
    $localDbOutput = & sqllocaldb info MSSQLLocalDB 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-OK "SQL Server LocalDB (MSSQLLocalDB)"
    } else {
        Write-Err "SQL Server LocalDB not found"
        Write-Info "LocalDB ships with Visual Studio or can be installed via SQL Server Express:"
        Write-Info "https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb"
        $allPrereqs = $false
    }
} catch {
    Write-Err "SQL Server LocalDB not found"
    Write-Info "Install via SQL Server Express LocalDB installer"
    $allPrereqs = $false
}

if (-not $allPrereqs) {
    Write-Host "`n❌ One or more prerequisites are missing. Install them and re-run.`n" -ForegroundColor Red
    exit 1
}

# ─── Phase 1: Database Migrations ────────────────────────────
$backendDir = Join-Path $scriptDir "source\02-backend"

if (-not $SkipMigrations -and -not $UiOnly) {
    Write-Step "Phase 1: Database Migrations (DbUp)"

    # Ensure LocalDB is started
    Write-Info "Starting SQL Server LocalDB..."
    & sqllocaldb start MSSQLLocalDB 2>$null

    # Build and run the migration project
    # In v0.1.2, DbUp is invoked via a dedicated console runner or direct inline.
    # For v0.1.2, we use sqlcmd to apply the migration script directly as a simple
    # alternative until DbUp is fully wired into the API startup.
    Write-Info "Applying migration: 001_CreateSchemaVersion.sql"

    $migrationScript = Join-Path $scriptDir "source\03-sql\migrations\001_CreateSchemaVersion.sql"
    $sqlcmdOutput = & sqlcmd -S "(LocalDB)\MSSQLLocalDB" -i $migrationScript -b 2>&1

    if ($LASTEXITCODE -eq 0) {
        Write-OK "Migration applied successfully (database: Synergistic)"

        # Generate scripted copy for AI model access
        $scriptOutputDir = Join-Path $scriptDir "source\03-sql\script"
        if (-not (Test-Path $scriptOutputDir)) {
            New-Item -ItemType Directory -Path $scriptOutputDir -Force | Out-Null
        }
        Write-Info "Scripted copy will be generated on first migration run"
    } else {
        Write-Err "Migration failed:"
        Write-Err $sqlcmdOutput
        Write-Info "Ensure SQL Server LocalDB is running and the script syntax is valid."
        exit 1
    }
} else {
    Write-Info "Skipping database migrations (--SkipMigrations flag set)"
}

# ─── Phase 2: Start .NET API ─────────────────────────────────
if (-not $UiOnly) {
    Write-Step "Phase 2: Starting .NET API (http://localhost:5001)"

    $apiProjectDir = Join-Path $backendDir "src\Api"
    Push-Location $apiProjectDir

    try {
        # Build first to catch compilation errors early
        Write-Info "Building..."
        & dotnet build --nologo -v q 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Err "API build failed. Check errors above."
            Pop-Location
            exit 1
        }

        Write-Info "Starting Kestrel on http://localhost:5001..."
        $apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --no-build" -PassThru -WindowStyle Minimized

        Write-OK ".NET API starting (PID: $($apiProcess.Id))"
        Write-Info "API will be available at http://localhost:5001/api/health"
    } finally {
        Pop-Location
    }
}

# ─── Phase 3: Start Angular Dev Server ───────────────────────
if (-not $ApiOnly) {
    Write-Step "Phase 3: Starting Angular Dev Server (http://localhost:4200)"

    $uiDir = Join-Path $scriptDir "source\01-ui"

    if (-not (Test-Path (Join-Path $uiDir "node_modules"))) {
        Write-Info "Installing npm dependencies..."
        Push-Location $uiDir
        npm install --silent 2>&1
        Pop-Location
        Write-OK "Dependencies installed"
    }

    Write-Info "Starting Angular dev server..."
    Write-Host "`n╔═══════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host   "║  ✅ Full stack running!                      ║" -ForegroundColor Green
    Write-Host   "║  Angular: http://localhost:4200              ║" -ForegroundColor Green
    Write-Host   "║  API:     http://localhost:5001/api/health    ║" -ForegroundColor Green
    Write-Host   "║  DB:      (LocalDB)\MSSQLLocalDB\Synergistic  ║" -ForegroundColor Green
    Write-Host   "║  Press Ctrl+C to stop all processes           ║" -ForegroundColor Green
    Write-Host   "╚═══════════════════════════════════════════════╝`n" -ForegroundColor Green

    Push-Location $uiDir
    try {
        npm start
    } finally {
        Pop-Location
        # Cleanup: stop the API process if it was started
        if ($apiProcess -and -not $apiProcess.HasExited) {
            Write-Info "Stopping .NET API (PID: $($apiProcess.Id))..."
            $apiProcess.Kill()
        }
    }
} else {
    Write-Host "`n╔═══════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host   "║  ✅ .NET API running                          ║" -ForegroundColor Green
    Write-Host   "║  API: http://localhost:5001/api/health         ║" -ForegroundColor Green
    Write-Host   "║  Press Ctrl+C to stop                         ║" -ForegroundColor Green
    Write-Host   "╚═══════════════════════════════════════════════╝`n" -ForegroundColor Green

    # Wait for the API process to keep the script alive
    if ($apiProcess) {
        $apiProcess.WaitForExit()
    }
}