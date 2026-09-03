$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $root "ProfessionalServicesHub_Book.slnx"
$appProject = Join-Path $root "ProfessionalServicesHub/ProfessionalServicesHub.csproj"
$publishDir = Join-Path $root "artifacts/publish"
$testResultsDir = Join-Path $root "artifacts/test-results"

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

    Invoke-Checked "Restore packages" {
        dotnet restore $solution
    }

    Invoke-Checked "Build Release" {
        dotnet build $solution `
            -c Release `
            --no-restore `
            -warnaserror
    }

    Invoke-Checked "Run automated tests" {
        dotnet test `
            --solution $solution `
            -c Release `
            --no-build `
            --results-directory $testResultsDir `
            -- `
            --report-trx
    }

    Invoke-Checked "Verify formatting" {
        dotnet format $solution `
            --verify-no-changes `
            --no-restore
    }

    Invoke-Checked "Verify EF model and migrations" {
        dotnet ef migrations has-pending-model-changes `
            --context ApplicationDbContext `
            --project $appProject `
            --startup-project $appProject `
            --no-build
    }

    if (Test-Path $publishDir) {
        Remove-Item $publishDir -Recurse -Force
    }

    Invoke-Checked "Publish Release artifact" {
        dotnet publish $appProject `
            -c Release `
            --no-build `
            -o $publishDir
    }
}
finally {
    Pop-Location
}
