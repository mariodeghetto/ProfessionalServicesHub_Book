#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SOLUTION="$ROOT/ProfessionalServicesHub_Book.slnx"
APP_PROJECT="$ROOT/ProfessionalServicesHub/ProfessionalServicesHub.csproj"
PUBLISH_DIR="$ROOT/artifacts/publish"

cd "$ROOT"

dotnet tool restore
dotnet restore "$SOLUTION"
dotnet build "$SOLUTION" -c Release --no-restore
dotnet test "$SOLUTION" -c Release --no-build --logger "trx"
dotnet format "$SOLUTION" --verify-no-changes --no-restore

dotnet ef migrations has-pending-model-changes \
  --context ApplicationDbContext \
  --project "$APP_PROJECT" \
  --startup-project "$APP_PROJECT" \
  --no-build

rm -rf "$PUBLISH_DIR"

dotnet publish "$APP_PROJECT" \
  -c Release \
  --no-build \
  -o "$PUBLISH_DIR"
