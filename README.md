# Professional Services Hub - Book Edition

Professional Services Hub is the companion application for the book
*Building Business Apps with Syncfusion Blazor*.

The project demonstrates how to build a professional business application
with .NET 10, ASP.NET Core, Blazor, Entity Framework Core, SQLite, and
Syncfusion Blazor components.

## Technology baseline

- .NET 10
- ASP.NET Core
- Blazor Web App
- Interactive Server render mode
- Entity Framework Core
- SQLite
- Syncfusion Blazor
- Fluent 2 theme

## Repository structure

The application is implemented as a single deployable project while keeping
clear logical boundaries between responsibilities:

- `Application` - application services and contracts
- `Domain` - business entities and domain concepts
- `Infrastructure` - persistence and infrastructure services
- `Components` - Blazor pages, layouts, and shared UI components
- `Data` - local runtime data such as the development SQLite database

The structure intentionally remains lightweight. The Book Edition does not
split these responsibilities into separate assemblies.

## Prerequisites

- .NET 10 SDK
- Git
- A valid Syncfusion license or trial
- Visual Studio, Visual Studio Code, or another compatible editor

Verify the installed SDK:

```text
dotnet --version
```

The repository currently targets SDK 10.0.400 through `global.json`.

## Syncfusion license

The Syncfusion license key is never stored in this repository.

For local development, configure it with .NET User Secrets:

```text
cd ProfessionalServicesHub
dotnet user-secrets set "Syncfusion:LicenseKey" "YOUR_LICENSE_KEY"
```

Alternatively, provide the environment variable:

```text
Syncfusion__LicenseKey
```

See `SETUP.md` for complete setup instructions.

## Build

From the repository root:

```text
dotnet restore ProfessionalServicesHub_Book.slnx
dotnet build ProfessionalServicesHub_Book.slnx
```

## Run

Use the HTTPS launch profile for local development:

```text
dotnet run --launch-profile https --project ProfessionalServicesHub/ProfessionalServicesHub.csproj
```

The current Book Edition milestone includes the application shell, responsive
Syncfusion Sidebar navigation, stable business routes, and reusable page
headers.

## Local database

The Book Edition uses SQLite as its default development database.

The configured path is:

```text
ProfessionalServicesHub/Data/professionalserviceshub.db
```

Runtime database files are excluded from Git.

The database schema is introduced in later chapters of the book.

## Package management

Package versions are managed centrally through:

```text
Directory.Packages.props
```

All Syncfusion packages must use the same Syncfusion release.

## License

The source code in this repository is licensed under the Apache License 2.0.
See `LICENSE`.

Syncfusion components are third-party commercial software and are not covered
by the Apache License 2.0 granted for this sample code.

Each user is responsible for obtaining and using a valid Syncfusion license
or trial according to Syncfusion licensing terms.

## Book Edition scope

This repository follows the architecture and implementation presented in
*Building Business Apps with Syncfusion Blazor*.

It is intentionally designed as an educational but executable business
application rather than as a catalog of isolated UI component examples.
