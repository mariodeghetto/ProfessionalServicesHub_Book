$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $root "ProfessionalServicesHub_Book.slnx"
$appProject = Join-Path $root "ProfessionalServicesHub/ProfessionalServicesHub.csproj"
$publishDir = Join-Path $root "artifacts/publish"

Push-Location $root

try {
    dotnet tool restore
    dotnet restore $solution
    dotnet build $solution -c Release --no-restore
    dotnet test $solution -c Release --no-build --logger "trx"
    dotnet format $solution --verify-no-changes --no-restore

    dotnet ef migrations has-pending-model-changes `
        --context ApplicationDbContext `
        --project $appProject `
        --startup-project $appProject `
        --no-build

    if (Test-Path $publishDir) {
        Remove-Item $publishDir -Recurse -Force
    }

    dotnet publish $appProject `
        -c Release `
        --no-build `
        -o $publishDir
}
finally {
    Pop-Location
}
