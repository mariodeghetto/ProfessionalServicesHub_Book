$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$appProject = Join-Path $root "ProfessionalServicesHub/ProfessionalServicesHub.csproj"
$databaseDir = Join-Path $root "artifacts/database"
$bundlePath = Join-Path $databaseDir "efbundle.exe"

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Description,

        [Parameter(Mandatory = $true)]
        [scriptblock] $Command
    )

    Write-Host ""
    Write-Host "==> $Description"

    & $Command

    if ($LASTEXITCODE -ne 0) {
        throw "$Description failed with exit code $LASTEXITCODE."
    }
}

Push-Location $root

try {
    Invoke-Checked "Restore local tools" {
        dotnet tool restore
    }

    New-Item `
        -ItemType Directory `
        -Path $databaseDir `
        -Force | Out-Null

    Invoke-Checked "Build EF migration bundle" {
        dotnet ef migrations bundle `
            --context ApplicationDbContext `
            --project $appProject `
            --startup-project $appProject `
            --configuration Release `
            --output $bundlePath `
            --force
    }
}
finally {
    Pop-Location
}
