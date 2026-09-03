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

The current migrations create the client model plus the Chapter 6
`Engagements` and `WorkActivities` tables, foreign keys, and indexes.

The Development seed is intentionally separate from schema migration. It runs
when the application starts, inserts deterministic sample clients only when
the Clients table is empty, and independently initializes the workflow slice
with two sample engagements and five work activities when those workflow
tables are empty.

## 7. Run the application

Use the HTTPS launch profile:

```text
dotnet run --launch-profile https --project ProfessionalServicesHub/ProfessionalServicesHub.csproj
```

Open the HTTPS URL displayed by ASP.NET Core.

At the Chapter 6 milestone, verify that:

- the Chapter 5 client grid and client editor behaviors remain operational
- `/tasks` displays the Planned, In progress, Waiting, and Completed Kanban columns
- the development seed produces five work-activity cards across Alex Morgan, Jordan Lee, and Unassigned swimlanes
- engagement codes such as `ENG-001` and `ENG-002` appear as the work context on task cards
- `/engagements` clearly states that engagement data already supports the task workflow while full engagement management is deferred
- Planned -> Completed is rejected by the workflow
- Planned -> In progress is accepted and remains persisted after refresh
- reordering within one column is rejected with an explicit message because Rank persistence is not implemented yet
- In progress -> Completed is accepted
- Completed -> Planned is rejected
- Completed -> In progress is accepted as an explicit reopen operation
- Waiting -> In progress and In progress -> Waiting are accepted
- no Syncfusion asset, license, Blazor, EF Core, or Kanban runtime error appears in the browser console or application log

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
