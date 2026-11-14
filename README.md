# CRM for Students

**CRM for Students** is a full-stack university student management, academic tracking, task productivity, and analytics web application built with **C#**, **.NET 10**, **Blazor Interactive Server**, **ASP.NET Core Web API**, **Entity Framework Core**, **SQLite**, and **xUnit**.

---

## 1. Architecture & Solution Structure

```text
CRM-System/
├── CRM-System.slnx
├── README.md
├── DELIVERY/
│   ├── README_STAGE_01.md / MANIFEST_STAGE_01.md
│   ├── README_STAGE_02.md / MANIFEST_STAGE_02.md
│   ├── README_STAGE_03.md / MANIFEST_STAGE_03.md
│   └── README_STAGE_04.md / MANIFEST_STAGE_04.md
├── src/
│   ├── CRM-System.Core/            # Domain entities, DTOs, interfaces, validators & business services
│   │   ├── Common/                 # AccessContext, ServiceResult, shared primitives
│   │   ├── Students/               # Stage 1: Users, Students, AuthService, StudentService
│   │   ├── Academic/               # Stage 2: Courses, Teachers, Enrollments, Grades, Schedule, Attendance, GPA
│   │   ├── Tasks/                  # Stage 3: StudentTasks, Notifications, Calendar events
│   │   └── Dashboard/              # Stage 4: Dashboard KPIs, Academic/Attendance/Task Analytics
│   ├── CRM-System.Infrastructure/  # EF Core SQLite DbContext, configurations, migrations, repositories & seeding
│   ├── CRM-System.API/             # ASP.NET Core Minimal Web API + OpenAPI endpoints
│   └── CRM-System.Web/             # Blazor Interactive Server UI (Bootstrap 5 + custom responsive theme)
└── tests/
    └── CRM-System.Tests/           # Automated xUnit unit & SQLite integration tests (39 tests across all 4 stages)
```

---

## 2. Four Functional Modules

| Stage | Module | Developer | Key Capabilities |
|---|---|---|---|
| **Stage 1** | **Student Management** | Developer 1 | Cookie authentication, PBKDF2 password hashing, role-based access (`Admin` / `Student`), student directory CRUD, search/filtering/sorting, profile & account settings |
| **Stage 2** | **Academic Management** | Developer 1 | Course catalog & teacher CRUD, student course enrollments, weighted grade management, 4.0-scale GPA calculation (`IGpaCalculator`), weekly class schedule, attendance tracking |
| **Stage 3** | **Tasks & Notifications** | Developer 2 | Student & admin task CRUD, priorities (`Low`/`Medium`/`High`), automatic overdue detection, monthly schedule + deadline calendar, persisted notifications & unread bell badge |
| **Stage 4** | **Dashboard & Analytics** | Developer 2 | Live KPI dashboard (`/`), dedicated Analytics Center (`/analytics`), GPA & grade distributions, course & student rankings, attendance session trends, task productivity & multi-dimensional filters |

---

## 3. Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (`10.0.100` or newer)

### Build the Solution
```bash
dotnet build CRM-System.slnx
```

### Run Automated Tests (39 xUnit Tests)
```bash
dotnet test CRM-System.slnx
```

### Run the Blazor Web Application
```bash
dotnet run --project src/CRM-System.Web
```
On startup, `DatabaseInitializer` automatically applies all Entity Framework Core SQLite migrations (`crm_students.db`) and seeds realistic university demo data across all 4 modules.

### Run the Standalone REST Web API (Optional)
```bash
dotnet run --project src/CRM-System.API
```

---

## 4. Demo Accounts

| Role | Email | Password | Capabilities |
|---|---|---|---|
| **Administrator** | `admin@crm.edu` | `Admin123!` | Full university management, student/course/teacher CRUD, grading, attendance recording, task assignment, university-wide analytics |
| **Student** | `aidar.nurlanov@student.crm.edu` | `Student123!` | Personal dashboard & analytics, enrolled courses, GPA & transcript, weekly schedule, attendance history, personal tasks, calendar & notifications |
| **Student** | `dana.suleimenova@student.crm.edu` | `Student123!` | Personal student portal |
| **Student** | `timur.kassymov@student.crm.edu` | `Student123!` | Personal student portal |
| **Student** | `aliya.bekova@student.crm.edu` | `Student123!` | Personal student portal |
| **Student** | `maxim.orlov@student.crm.edu` | `Student123!` | Personal student portal |

---

## 5. Key Web Routes & REST API Endpoints

### Blazor Web Routes (`CRM-System.Web`)
- `/` — **Stage 4 Main Dashboard** (KPI cards, live filters, GPA/grade bars, attendance trend, tasks, deadlines, notifications, recent activity)
- `/analytics` (`/dashboard/analytics`) — **Stage 4 Analytics Center** (Academic & GPA, Attendance, Tasks & Productivity, Course & Student rankings)
- `/login` & `/register` — **Stage 1 Authentication**
- `/students` — **Stage 1 Student Directory**
- `/profile` & `/settings` — **Stage 1 Personal Profile & Security Settings**
- `/courses` — **Stage 2 Courses, Teachers & Enrollments**
- `/academic-performance` — **Stage 2 Grades, Transcript & 4.0 GPA Calculator**
- `/schedule` — **Stage 2 Weekly Class Schedule**
- `/attendance` — **Stage 2 Attendance Tracker**
- `/tasks` — **Stage 3 Tasks & Deadlines Manager**
- `/calendar` — **Stage 3 Monthly Academic & Deadline Calendar**
- `/notifications` — **Stage 3 Notification Center**

### REST API Groups (`CRM-System.API`)
- `/api/auth/*` & `/api/students/*` — Authentication & Student Management
- `/api/academic/*` — Teachers, Courses, Enrollments, Grades, GPA, Schedule & Attendance
- `/api/tasks/*`, `/api/calendar/*` & `/api/notifications/*` — Tasks, Calendar & Notifications
- `/api/dashboard/overview`, `/api/dashboard/analytics/academic`, `/api/dashboard/analytics/attendance`, `/api/dashboard/analytics/tasks` — Dashboard & Analytics
