#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
APP_PROJECT="$ROOT/ProfessionalServicesHub/ProfessionalServicesHub.csproj"
DATABASE_DIR="$ROOT/artifacts/database"
BUNDLE_PATH="$DATABASE_DIR/efbundle"

cd "$ROOT"

dotnet tool restore
mkdir -p "$DATABASE_DIR"

dotnet ef migrations bundle \
  --context ApplicationDbContext \
  --project "$APP_PROJECT" \
  --startup-project "$APP_PROJECT" \
  --configuration Release \
  --output "$BUNDLE_PATH" \
  --force
