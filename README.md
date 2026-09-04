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

The current Book Edition milestone includes the application shell, scoped
client and engagement access, operational task workflow, calendar scheduling,
a private document repository, an operational dashboard, shared presentation
feedback patterns, and ASP.NET Core Identity-based authentication and
authorization. The
Clients page uses a Syncfusion DataGrid for exploration and selection, while a
reusable client editor supports creation and modification with Syncfusion
inputs, Blazor validation, database-backed duplicate-code checks, unsaved-change
protection, and explicit loading and error states.

The Tasks page uses Syncfusion Kanban to project work activities across the
Planned, In progress, Waiting, and Completed workflow states. Engagements
provide the work context for activities, swimlanes group cards by assignee,
and workflow transitions are validated by the application layer before an
atomic state update is persisted. Reordering within one column is intentionally
not persisted yet. The Engagements page provides a scoped read-only list of
professional engagements. Full engagement editing is intentionally outside the
Book Edition.

The Calendar page uses Syncfusion Scheduler with Day, Week, WorkWeek, Month,
and Agenda views. Calendar entries are loaded by time range and persisted
through an application service rather than directly by the component. The
sample distinguishes appointments from deadlines, supports drag and drop,
resizing, creation, editing, and deletion, and rejects overlapping timed
appointments for the same assignee. All-day deadlines use a compact two-line
template so the subject and entry type remain readable in the all-day band.

The Documents page combines Syncfusion File Upload, DataGrid, and PDF Viewer
with an application service and a private storage abstraction. Document bytes
are stored under `App_Data/documents`, outside `wwwroot`, while business
metadata is stored in SQLite. Uploads are limited to PDF, DOCX, XLSX, PNG, and
JPG files up to 20 MB. The application validates extension, actual byte count,
basic file signatures, and OOXML package structure, calculates SHA-256, and
uses compensating cleanup if metadata persistence fails. The repository can
be filtered by engagement, PDFs can be previewed without exposing the storage
key, downloads resolve by business document ID, and archiving removes a
document from active views without deleting its physical file.

The root route now hosts an operational dashboard backed by a dedicated
`DashboardSnapshot` read model and `DashboardService`. Entity Framework Core
queries calculate open-work KPIs, overdue activities, upcoming deadlines,
recent documents, workflow-state counts, a fourteen-day deadline trend, and
open activities by assignee. Syncfusion Charts renders column, line, and
accumulation visualizations, while KPI cards remain lightweight HTML and CSS.
The dashboard supports manual refresh with a visible completion timestamp and
uses a responsive layout that collapses charts before they become cramped.

Chapter 10 adds reusable presentation-layer UX services and components without
moving UI concerns into the Application or Domain layers. A scoped
`UiNotificationService` publishes consistent toast notifications through one
global `AppToastHost`, while one `SfDialogProvider` supports confirmation
dialogs. Client saving uses a visible busy state with Syncfusion Spinner,
calendar deletion requires explicit confirmation, document archive exposes a
secondary tooltip and success notifications, and successful Kanban transitions
publish concise feedback. The Clients page also adds a server-side
`SfAutoComplete` lookup with a two-character minimum, 300 ms debounce, a
20-result limit, and cancellation of stale searches.

Chapter 11 adds ASP.NET Core Identity to the existing EF Core persistence
without introducing a second business DbContext. Business pages require an
authenticated user by default, while the Account area remains in static SSR so
login and logout can safely write Identity cookies. Administrator, Coordinator,
and Collaborator roles are provisioned without repository secrets, and
application policies represent global capabilities.

Data visibility is enforced before materialization. `EngagementAssignment`
links users to engagements, while reusable scope queries filter engagements,
clients, work activities, calendar entries, documents, and dashboard
aggregations. Administrator and Coordinator have global operational scope;
Collaborator sees only assigned engagement data. Observer assignments remain
readable but cannot modify scoped resources. Document metadata, preview,
archive, upload, and HTTP download all apply the same access rules before
opening private storage streams.

Development provisioning is opt-in through .NET User Secrets. An Administrator
can be created from `DemoIdentity:AdministratorEmail` and
`DemoIdentity:AdministratorPassword`. A Collaborator can optionally be
created from `DemoIdentity:CollaboratorEmail` and
`DemoIdentity:CollaboratorPassword`; the development seed assigns that user
to `ENG-001` as a Collaborator when the engagement exists.

## Local database

The Book Edition uses SQLite as its default development database.

The configured path is:

```text
ProfessionalServicesHub/Data/professionalserviceshub.db
```

Runtime database files and private document content under
`ProfessionalServicesHub/App_Data` are excluded from Git. The EF Core
migration files are versioned in:

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
are empty. The calendar seed then adds two appointments and one all-day
deadline when the CalendarEntries table is empty. The document repository is
not seeded; Chapter 8 verification uses real local uploads.

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

All Syncfusion packages must use the same Syncfusion release. Chapter 9 adds
`Syncfusion.Blazor.Charts` for the operational dashboard visualizations.
Chapter 10 adds `Syncfusion.Blazor.Notifications`,
`Syncfusion.Blazor.Popups`, and `Syncfusion.Blazor.Spinner`; the existing
DropDowns package is reused for `SfAutoComplete`. Chapter 11 adds
`Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.0.11 and keeps Identity
on the same `ApplicationDbContext` used by the business model.

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
