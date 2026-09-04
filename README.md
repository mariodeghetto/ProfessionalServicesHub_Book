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
dotnet tool restore
dotnet restore ProfessionalServicesHub_Book.slnx
dotnet build ProfessionalServicesHub_Book.slnx
```

## Run

Use the HTTPS launch profile for local development:

```text
dotnet run --launch-profile https --project ProfessionalServicesHub/ProfessionalServicesHub.csproj
```

The current Book Edition milestone includes the application shell, complete
client management, and the first operational workflow slice. The Clients page
uses a Syncfusion DataGrid for exploration and selection, while a reusable
client editor supports creation and modification with Syncfusion inputs,
Blazor validation, database-backed duplicate-code checks, unsaved-change
protection, and explicit loading and error states.

The Tasks page uses Syncfusion Kanban to project work activities across the
Planned, In progress, Waiting, and Completed workflow states. Engagements
provide the work context for activities, swimlanes group cards by assignee,
and workflow transitions are validated by the application layer before an
atomic state update is persisted. Reordering within one column is intentionally
not persisted yet. The Engagements page remains informational in these
milestones. Full engagement editing is intentionally outside the Book Edition;
Chapter 11 later adds scoped read-only engagement browsing.

## Local database

The Book Edition uses SQLite as its default development database.

The configured path is:

```text
ProfessionalServicesHub/Data/professionalserviceshub.db
```

Runtime database files are excluded from Git. The EF Core migration files are
versioned in:

```text
ProfessionalServicesHub/Infrastructure/Data/Migrations
```

Apply the current schema from the repository root:

```text
dotnet tool restore
dotnet ef database update --project ProfessionalServicesHub/ProfessionalServicesHub.csproj --startup-project ProfessionalServicesHub/ProfessionalServicesHub.csproj
```

In Development, deterministic client seed data is inserted only when the
Clients table is empty. The workflow seed independently creates two sample
engagements and five work activities when the corresponding workflow tables
are empty.

## Client form validation

The client editor uses an edit model rather than binding directly to the EF
Core entity. Immediate field rules are enforced through DataAnnotations and
the custom business email validator. The email policy used by this sample
requires an Internet-style public domain, so addresses such as
`name@sub.example.com` are accepted while `name@example` is rejected.

Client-code uniqueness is enforced by both the application service and the
database unique index.

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
