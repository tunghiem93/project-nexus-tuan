# Initialize SQL Server databases for Project Nexus
# Requires: docker compose up -d (sqlserver healthy)

$ErrorActionPreference = "Stop"

$Server = "localhost,1433"
$User = "sa"
$Password = "Nexus_Dev_2026!"
$Root = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent

function Invoke-SqlFile {
    param([string]$Database, [string]$FilePath)
    Write-Host "Applying $FilePath -> $Database ..."
    sqlcmd -S $Server -U $User -P $Password -C -d $Database -i $FilePath
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed for $FilePath" }
}

Write-Host "Waiting for SQL Server..."
$ready = $false
for ($i = 0; $i -lt 30; $i++) {
    sqlcmd -S $Server -U $User -P $Password -C -Q "SELECT 1" 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) { $ready = $true; break }
    Start-Sleep -Seconds 2
}
if (-not $ready) { throw "SQL Server not ready on $Server" }

Invoke-SqlFile -Database "master" -FilePath (Join-Path $PSScriptRoot "01-create-databases.sql")

$schemas = @(
    @{ Db = "Nexus_User";          File = "services/user-service/db/schema.sql" },
    @{ Db = "Nexus_Catalog";       File = "services/catalog-service/db/schema.sql" },
    @{ Db = "Nexus_Commerce";      File = "services/commerce-service/db/schema.sql" },
    @{ Db = "Nexus_Auction";       File = "services/auction-service/db/schema.sql" },
    @{ Db = "Nexus_Fulfillment";   File = "services/fulfillment-service/db/schema.sql" },
    @{ Db = "Nexus_Notification";  File = "services/notification-service/db/schema.sql" }
)

foreach ($s in $schemas) {
    Invoke-SqlFile -Database $s.Db -FilePath (Join-Path $Root $s.File)
}

Write-Host "All schemas applied successfully."
