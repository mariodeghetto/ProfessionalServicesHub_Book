# Development Setup

This document describes how to configure and run the Professional Services
Hub Book Edition locally.

## 1. Clone the repository

```text
git clone https://github.com/mariodeghetto/ProfessionalServicesHub_Book.git
cd ProfessionalServicesHub_Book
```

## 2. Verify the .NET SDK

Run:

```text
dotnet --version
```

The repository contains `global.json` and currently targets .NET SDK
10.0.400 with patch roll-forward enabled.

## 3. Restore tools and packages

From the repository root:

```text
dotnet tool restore
dotnet restore ProfessionalServicesHub_Book.slnx
```

The local tool manifest pins the EF Core CLI tool used to manage migrations.

Package versions are defined centrally in `Directory.Packages.props`.

Do not assign independent Syncfusion versions inside individual project
files. All Syncfusion packages must remain aligned to the same release.

## 4. Configure the Syncfusion license

A valid Syncfusion license or trial is required.

The license key must not be added to:

- source code
- `appsettings.json`
- `appsettings.Development.json`
- committed environment files
- documentation examples containing real credentials

For local development:

```text
cd ProfessionalServicesHub
dotnet user-secrets set "Syncfusion:LicenseKey" "YOUR_LICENSE_KEY"
```

To verify that a value exists without displaying its contents:

```text
dotnet user-secrets list
```

For hosted environments, the same configuration key can be supplied through
an environment variable:

```text
Syncfusion__LicenseKey
```

ASP.NET Core maps the double underscore to the configuration separator, so
this value becomes:

```text
Syncfusion:LicenseKey
```

The application intentionally fails during startup when the license key is
missing.

## 5. Build the solution

From the repository root:

```text
dotnet build ProfessionalServicesHub_Book.slnx
```

The build must complete without errors before proceeding.

## 6. Apply the development database schema

The Book Edition uses SQLite for local development.

From the repository root:

```text
dotnet ef database update --project ProfessionalServicesHub/ProfessionalServicesHub.csproj --startup-project ProfessionalServicesHub/ProfessionalServicesHub.csproj
```

The current migration creates the `Clients` table and its indexes.

The Development seed is intentionally separate from schema migration. It runs
when the application starts and inserts deterministic sample clients only
when the Clients table is empty.

## 7. Run the application

Use the HTTPS launch profile:

```text
dotnet run --launch-profile https --project ProfessionalServicesHub/ProfessionalServicesHub.csproj
```

Open the HTTPS URL displayed by ASP.NET Core.

At the Chapter 5 milestone, verify that:

- the Clients page still supports sorting, search, filtering, paging, and single-row selection
- New client opens `/clients/new` with an empty edit model and Active status
- required Code and Name validation prevents invalid submission
- the business email policy rejects `name@example` and accepts multi-label domains such as `name@sub.example.com`
- a duplicate client code is reported without losing the entered form data
- a valid client can be created and appears in the Clients grid
- Open navigates to `/clients/{id}` and loads the existing values for editing
- changes to an existing client are persisted and reflected in the Clients grid
- an unknown client identifier produces an application message rather than an unhandled exception
- Save is disabled while a save operation is already in progress
- leaving a modified form triggers an unsaved-change confirmation
- confirming Cancel does not submit or persist the modified form
- no Syncfusion asset, license, Blazor, or EF Core runtime error appears in the browser console or application log

If the local HTTPS development certificate is not trusted, run:

```text
dotnet dev-certs https --trust
```

and then start the application again.

## 8. Development database files

The default development connection string points to:

```text
Data/professionalserviceshub.db
```

relative to the application project.

The `Data` directory contains local runtime data.

SQLite database files and their auxiliary files are excluded from Git:

```text
*.db
*.db-shm
*.db-wal
```

Migration source files are versioned under:

```text
Infrastructure/Data/Migrations
```

## 9. Configuration principles

The repository follows these rules:

- secrets remain outside source control
- runtime data remains outside source control
- package and tool versions are centrally or locally pinned
- the repository must build from a clean clone
- the application must remain executable at chapter milestones
- source code, identifiers, comments, UI strings, technical documentation,
  and commit messages use American English

## 10. Basic local verification

Before committing a milestone, run:

```text
dotnet tool restore
dotnet restore ProfessionalServicesHub_Book.slnx
dotnet build ProfessionalServicesHub_Book.slnx -c Release --no-restore
dotnet format ProfessionalServicesHub_Book.slnx --verify-no-changes
git diff --check
git status
```

Later chapters extend this quality gate with automated tests, publishing, and
additional smoke testing.
