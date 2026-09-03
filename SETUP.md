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

The current migrations create the client model, the Chapter 6
`Engagements` and `WorkActivities` tables, the Chapter 7
`CalendarEntries` table, and the Chapter 8 `Documents` table with its
business relationships and indexes. Chapters 9 and 10 add no schema changes.
Chapter 11 adds migration `20260903193000_AddIdentityAndAccessScope`, which
creates the standard ASP.NET Core Identity tables and
`EngagementAssignments` while preserving the existing business tables.

The Development seed is intentionally separate from schema migration. It runs
when the application starts, inserts deterministic sample clients only when
the Clients table is empty, independently initializes the workflow slice with
two sample engagements and five work activities, and finally creates two
calendar appointments plus one all-day deadline when the CalendarEntries
table is empty. Documents are intentionally not seeded.

## 7. Run the application

### 7.1 Configure development identities

Chapter 11 does not commit demo passwords. To provision a local Administrator,
store the credentials in .NET User Secrets:

```text
dotnet user-secrets set --project ProfessionalServicesHub "DemoIdentity:AdministratorEmail" "admin@example.com"
dotnet user-secrets set --project ProfessionalServicesHub "DemoIdentity:AdministratorPassword" "YOUR_LOCAL_PASSWORD"
```

To exercise data scope, optionally provision a Collaborator:

```text
dotnet user-secrets set --project ProfessionalServicesHub "DemoIdentity:CollaboratorEmail" "collaborator@example.com"
dotnet user-secrets set --project ProfessionalServicesHub "DemoIdentity:CollaboratorPassword" "YOUR_LOCAL_PASSWORD"
```

On Development startup the roles Administrator, Coordinator, and Collaborator
are ensured. When collaborator credentials are present, the demo Collaborator
is assigned to `ENG-001` with `AssignmentKind.Collaborator`.

### 7.2 Start the application

Use the HTTPS launch profile:

```text
dotnet run --launch-profile https --project ProfessionalServicesHub/ProfessionalServicesHub.csproj
```

Open the HTTPS URL displayed by ASP.NET Core.

At the Chapter 11 milestone, verify that:

- the Chapter 5 client grid and client editor behaviors remain operational
- the Chapter 6 Kanban workflow remains operational and persisted
- the primary navigation remains fully visible on short-content pages without an artificial vertical scrollbar
- `/calendar` opens in WorkWeek view and displays the deterministic calendar seed
- the seed includes `Accessibility review meeting`, `Reporting workshop`, and `Send accessibility findings`
- Day, Week, WorkWeek, Month, and Agenda navigation loads the appropriate time window
- appointment creation persists after refresh
- deadline creation persists as an all-day entry after refresh
- all-day entries display the subject and entry type on two readable lines
- dragging an appointment to another free time persists the new interval
- resizing an appointment persists the new end time
- editing through the Scheduler editor persists the same validated data
- deleting a calendar entry removes it permanently after refresh
- overlapping timed appointments for the same assignee are rejected by the application service
- invalid intervals with EndTime less than or equal to StartTime are rejected
- no Syncfusion asset, license, Blazor, EF Core, Kanban, Scheduler, Uploader, DataGrid, or PDF Viewer runtime error appears in the browser console or application log
- `/documents` opens and the document repository loads
- a valid DOCX and a valid PDF below 20 MB can be uploaded
- uploaded bytes are stored under `ProfessionalServicesHub/App_Data/documents`, never under `wwwroot`
- uploaded documents appear in the grid with coherent metadata
- a document associated with `ENG-001` displays `ENG-001` and `Alpine Design`, and the engagement filter limits the grid correctly
- selecting a PDF loads it in Syncfusion PDF Viewer
- Download returns the document with its original file name
- Archive removes the document from the active repository without deleting the physical file
- a non-allowed extension such as `.txt` is rejected before persistence
- a renamed non-PDF file with a `.pdf` extension is rejected by server-side content validation
- upload errors are displayed in the prominent status banner at the top of the page
- `/` displays the operational dashboard instead of the earlier placeholder
- five KPI cards show engagements with open work, open activities, overdue activities, deadlines in seven days, and recent documents
- the activities-by-status chart displays all four workflow states, including zero-count states
- the fourteen-day deadline trend includes every day in the period, including days with zero deadlines
- the open-activities-by-assignee accumulation chart includes unassigned work when present
- dashboard refresh updates the snapshot and the visible `Last refreshed at` timestamp
- the dashboard charts remain readable when the browser width is reduced and collapse to a single column before becoming cramped
- no Syncfusion Charts, Blazor, or EF Core runtime error appears while loading or refreshing the dashboard
- client save disables the Save action while processing and exposes a visible busy state with Syncfusion Spinner
- a successful client save publishes a toast notification before returning to the client list
- the Clients page quick lookup searches by client code or name after at least two characters and opens the selected client
- client lookup uses server-side filtering with debounce, limits results, and does not replace persistent errors with transient toast feedback
- successful document upload publishes a toast while validation and technical failures remain visible inline
- the document Archive action exposes a tooltip and archiving publishes an informational toast
- deleting a calendar entry first displays a confirmation dialog; canceling preserves the entry and confirming deletes it
- successful calendar deletion publishes an informational toast
- a successful Kanban workflow transition publishes a toast and concurrent duplicate moves are prevented in the UI
- one global toast host and one dialog provider serve the interactive layout
- no Syncfusion Notifications, Popups, Spinner, AutoComplete, Blazor, or EF Core runtime error appears in the browser console or application log
- an anonymous request to `/` is redirected to `/account/login`
- the Account routes render in static SSR and successful sign-in issues the Identity cookie
- the application shell displays the authenticated user and exposes Sign out
- Administrator can access all seeded clients, engagements, tasks, calendar entries, documents, and dashboard data
- the optional Collaborator account is assigned only to `ENG-001`
- Collaborator sees only `ENG-001` in Engagements
- Collaborator sees only tasks and calendar entries linked to `ENG-001`
- Collaborator sees only clients reachable through visible engagements
- Collaborator cannot open the client editor or use client-management actions
- Collaborator sees only documents in visible engagements and cannot download out-of-scope document content
- Collaborator dashboard KPIs and charts are calculated only from visible data
- Collaborator may modify scoped resources only when the assignment is not Observer
- general calendar entries and general documents remain outside Collaborator scope in the current sample
- logout ends the session and protected business routes require sign-in again
- `dotnet ef migrations has-pending-model-changes` reports no pending model changes after the Chapter 11 migration
- no Identity, authorization, antiforgery, Blazor, EF Core, or Syncfusion runtime error appears during the Administrator and Collaborator smoke tests

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

The `Data` directory contains local database runtime data.

Chapter 8 document bytes are stored locally in:

```text
App_Data/documents
```

relative to the application project. This directory is private application
storage and is not served as a static web asset.

SQLite database files, their auxiliary files, and Chapter 8 document storage
are excluded from Git:

```text
*.db
*.db-shm
*.db-wal
ProfessionalServicesHub/App_Data/
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
