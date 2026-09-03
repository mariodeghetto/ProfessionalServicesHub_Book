$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$appProject = Join-Path $root "ProfessionalServicesHub/ProfessionalServicesHub.csproj"
$databaseDir = Join-Path $root "artifacts/database"
$bundlePath = Join-Path $databaseDir "efbundle.exe"

Push-Location $root

try {
    dotnet tool restore

    New-Item `
        -ItemType Directory `
        -Path $databaseDir `
        -Force | Out-Null

    dotnet ef migrations bundle `
        --context ApplicationDbContext `
        --project $appProject `
        --startup-project $appProject `
        --configuration Release `
        --output $bundlePath `
        --force
}
finally {
    Pop-Location
}
