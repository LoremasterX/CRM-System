# CRM for Students

A university-focused student management and academic productivity platform built with **C#**, **.NET 10**, **Blazor Interactive Server**, **ASP.NET Core Minimal API**, **Entity Framework Core**, **SQLite**, and **xUnit**.

The project brings student records, academic tracking, attendance, tasks, notifications, and analytics into one application.

> **Repository:** [LoremasterX/CRM-System](https://github.com/LoremasterX/CRM-System)  
> **Solution:** `CRM-System.slnx`  
> **Target framework:** `.NET 10`

---

## Table of contents

- [Project overview](#project-overview)
- [Features](#features)
- [Technology stack](#technology-stack)
- [Architecture](#architecture)
- [Getting started](#getting-started)
- [Demo accounts](#demo-accounts)
- [Application pages](#application-pages)
- [REST API](#rest-api)
- [Database and demo data](#database-and-demo-data)
- [Build and tests](#build-and-tests)
- [Team contributions](#team-contributions)
- [Project structure](#project-structure)

---

## Project overview

Universities often manage student profiles, courses, grades, attendance, and day-to-day academic tasks across separate systems or documents. **CRM for Students** provides a unified web application for these workflows.

The system supports two main roles:

- **Administrator** — manages student and academic records and has access to university-level information.
- **Student** — views personal academic information and manages their own tasks, schedule, and notifications.

The application is organized into four functional modules developed in stages.

## Features

### 1. Student Management

- Student registration and login.
- Cookie-based authentication and role-based access.
- Password hashing using PBKDF2.
- Student directory with create, read, update, and delete operations.
- Search, filtering, and sorting.
- Student profiles and account settings.

### 2. Academic Management

- Course and teacher management.
- Student course enrollment.
- Grade recording and weighted grade calculations.
- Configurable GPA calculation on a 4.0 scale.
- Weekly class schedule.
- Attendance tracking and attendance history.

### 3. Tasks & Notifications

- Create and manage student tasks.
- Task priorities: Low, Medium, and High.
- Task status and overdue tracking.
- Calendar view for academic events and deadlines.
- Persisted notifications and unread notification indicator.

### 4. Dashboard & Analytics

- Main dashboard with operational and academic statistics.
- Dedicated analytics center.
- GPA and grade distributions.
- Course and student performance summaries.
- Attendance breakdowns and trends.
- Task productivity, priorities, and deadline summaries.
- Filters for relevant academic and reporting dimensions.

Dashboard and analytics data are calculated from application data rather than being presented as static dashboard-only examples.

---

## Technology stack

| Area | Technology | Purpose |
|---|---|---|
| Language | C# | Application and business logic |
| Runtime | .NET 10 | Application platform |
| Web UI | Blazor Interactive Server | Interactive browser interface |
| API | ASP.NET Core Minimal API | HTTP endpoints |
| Data access | Entity Framework Core | ORM and database access |
| Database | SQLite | Persistent relational storage |
| Testing | xUnit | Automated unit and integration-oriented tests |
| UI styling | Bootstrap 5 + custom CSS | Responsive layout and styling |

---

## Architecture

The solution separates domain logic, infrastructure, HTTP endpoints, and the web interface.

```text
CRM-System/
├── CRM-System.slnx
├── README.md
├── DELIVERY/
│   ├── README_STAGE_01.md
│   ├── MANIFEST_STAGE_01.md
│   ├── README_STAGE_02.md
│   ├── MANIFEST_STAGE_02.md
│   ├── README_STAGE_03.md
│   ├── MANIFEST_STAGE_03.md
│   ├── README_STAGE_04.md
│   └── MANIFEST_STAGE_04.md
├── src/
│   ├── CRM-System.Core/
│   │   ├── Common/
│   │   ├── Students/
│   │   ├── Academic/
│   │   ├── Tasks/
│   │   └── Dashboard/
│   ├── CRM-System.Infrastructure/
│   │   ├── Data/
│   │   ├── Migrations/
│   │   └── ...
│   ├── CRM-System.API/
│   │   └── Endpoints/
│   └── CRM-System.Web/
│       └── Components/
└── tests/
    └── CRM-System.Tests/
        ├── Students/
        ├── Academic/
        ├── Tasks/
        └── Dashboard/
```

### Main responsibilities

- **Core** — domain models, DTOs, service interfaces, validation, and business rules.
- **Infrastructure** — Entity Framework Core DbContext, SQLite configuration, migrations, repositories, and database initialization.
- **API** — ASP.NET Core Minimal API endpoints.
- **Web** — Blazor pages, layouts, navigation, and interactive user experience.
- **Tests** — automated tests for student, academic, task, notification, and dashboard functionality.

The application uses a layered design so that business rules are not tied directly to the Blazor UI or database implementation.

---

## Getting started

### Prerequisites

- Windows, macOS, or Linux.
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) installed.
- Git (to clone the repository).

Check the installed SDK:

```bash
dotnet --version
```

The SDK should report a .NET 10 version.

### 1. Clone the repository

```bash
git clone https://github.com/LoremasterX/CRM-System.git
cd CRM-System
```

### 2. Restore dependencies

```bash
dotnet restore CRM-System.slnx
```

### 3. Build the solution

```bash
dotnet build CRM-System.slnx
```

### 4. Run automated tests

```bash
dotnet test CRM-System.slnx
```

The project currently includes **39 xUnit tests** covering the four functional modules. Test results may change as the codebase evolves.

### 5. Run the Blazor web application

From the repository root:

```bash
dotnet run --project src/CRM-System.Web
```

Open the local URL printed in the console. In the verified local Development run, the application listened on:

```text
http://localhost:5106
```

The port may differ depending on the launch profile and local configuration.

### 6. Run the REST API separately (optional)

```bash
dotnet run --project src/CRM-System.API
```

Use the API project's configured launch URL and the HTTP request examples in `src/CRM-System.API/CRM-System.API.http`.

---

## Demo accounts

The Development database initializer seeds demo users. The following credentials are intended for local demonstration:

| Role | Email | Password |
|---|---|---|
| Administrator | `admin@crm.edu` | `Admin123!` |
| Student | `aidar.nurlanov@student.crm.edu` | `Student123!` |
| Student | `dana.suleimenova@student.crm.edu` | `Student123!` |
| Student | `timur.kassymov@student.crm.edu` | `Student123!` |
| Student | `aliya.bekova@student.crm.edu` | `Student123!` |
| Student | `maxim.orlov@student.crm.edu` | `Student123!` |

**Security note:** These are demonstration credentials, not production credentials. Do not deploy the application with seeded passwords or development settings exposed to real users. Change or disable demo accounts and configure secure secrets before any production use.

---

## Application pages

The following routes are implemented in the Blazor web project:

| Route | Description |
|---|---|
| `/` | Main dashboard |
| `/analytics` | Analytics center |
| `/dashboard/analytics` | Alternate analytics route |
| `/login` | Login |
| `/register` | Student registration |
| `/students` | Student directory |
| `/profile` | Personal profile |
| `/settings` | Account settings |
| `/courses` | Courses, teachers, and enrollments |
| `/academic-performance` | Grades, transcript, and GPA |
| `/schedule` | Weekly schedule |
| `/attendance` | Attendance records |
| `/tasks` | Tasks and deadlines |
| `/calendar` | Calendar |
| `/notifications` | Notification center |

Access to role-restricted pages and records depends on the authenticated user's role.

---

## REST API

The API project exposes Minimal API endpoint groups for the main functional areas.

| Endpoint group | Responsibility |
|---|---|
| `/api/auth/*` | Authentication |
| `/api/students/*` | Student management |
| `/api/academic/*` | Courses, teachers, enrollments, grades, GPA, schedule, and attendance |
| `/api/tasks/*` | Task management |
| `/api/calendar/*` | Calendar and deadlines |
| `/api/notifications/*` | Notifications |
| `/api/dashboard/overview` | Dashboard overview |
| `/api/dashboard/analytics/academic` | Academic analytics |
| `/api/dashboard/analytics/attendance` | Attendance analytics |
| `/api/dashboard/analytics/tasks` | Task analytics |

For request examples, see `src/CRM-System.API/CRM-System.API.http`.

---

## Database and demo data

The application uses SQLite through Entity Framework Core.

During application startup, the database initializer applies the available EF Core migrations and, when required, seeds demonstration data for the modules. The local Development run confirmed that the following data categories were initialized:

- Initial CRM users and student profiles.
- Academic data: teachers, courses, enrollments, grades, schedule, and attendance.
- Tasks and notifications.

The database file is created as part of the local application setup. Keep local database files and any environment-specific data out of source control unless there is a specific project requirement to include them.

---

## Build and tests

Run these commands from the repository root:

```bash
dotnet restore CRM-System.slnx
dotnet build CRM-System.slnx
dotnet test CRM-System.slnx
```

The latest local verification recorded for the completed four-stage solution:

- Build: succeeded.
- Automated tests: **39 passed, 0 failed, 0 skipped**.

NuGet may display advisory warnings for transitive SQLite native package dependencies depending on the restored package graph. Review current package advisories and update affected dependencies before production deployment.

---

## Team contributions

The project was developed by a two-person team in four stages.

| Developer | Modules | Main responsibilities |
|---|---|---|
| Developer 1 | Stage 1 — Student Management; Stage 2 — Academic Management | Student accounts and profiles, authentication, academic records, courses, teachers, enrollments, grades, GPA, schedule, and attendance |
| Developer 2 | Stage 3 — Tasks & Notifications; Stage 4 — Dashboard & Analytics | Task and notification workflows, calendar and deadlines, dashboard statistics, and cross-module analytics |

Both team members should be prepared to explain the overall architecture, integration between modules, and the features demonstrated during the project defence.

---

## Project structure

- `src/CRM-System.Core` — domain and business logic.
- `src/CRM-System.Infrastructure` — persistence and infrastructure.
- `src/CRM-System.API` — REST API.
- `src/CRM-System.Web` — Blazor web application.
- `tests/CRM-System.Tests` — automated tests.
- `DELIVERY/` — stage-specific delivery notes and manifests.

## License

No license file is currently specified in this repository. Unless a license is added, the project should be treated as source available for review but not automatically licensed for reuse or redistribution.
