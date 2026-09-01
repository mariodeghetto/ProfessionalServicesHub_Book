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

## 3. Restore packages

From the repository root:

```text
dotnet restore ProfessionalServicesHub_Book.slnx
```

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

## 6. Run the application

Use the HTTPS launch profile:

```text
dotnet run --launch-profile https --project ProfessionalServicesHub/ProfessionalServicesHub.csproj
```

Open the HTTPS URL displayed by ASP.NET Core.

At the Chapter 3 milestone, verify that:

- the application shell is displayed
- the Syncfusion Sidebar opens and closes correctly
- Dashboard, Clients, Engagements, Tasks, Calendar, and Documents are reachable
- the active navigation item follows the current route
- the shell remains usable at smaller viewport widths
- no Syncfusion asset or license warning appears in the browser console

If the local HTTPS development certificate is not trusted, run:

```text
dotnet dev-certs https --trust
```

and then start the application again.

## 7. Development database

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

The initial bootstrap does not create a business schema. EF Core entities,
the application DbContext, migrations, and seed data are introduced in later
chapters.

## 8. Configuration principles

The repository follows these rules:

- secrets remain outside source control
- runtime data remains outside source control
- package versions are centrally managed
- the repository must build from a clean clone
- the application must remain executable at chapter milestones
- source code, identifiers, comments, UI strings, technical documentation,
  and commit messages use American English

## 9. Basic local verification

Before committing a milestone, run:

```text
dotnet restore ProfessionalServicesHub_Book.slnx
dotnet build ProfessionalServicesHub_Book.slnx
git diff --check
git status
```

Later chapters extend this quality gate with automated tests, Release builds,
format verification, publishing, and smoke testing.
