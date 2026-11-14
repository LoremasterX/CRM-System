# Stage 1 Delivery: Student Management (`01_Student_Management.zip`)

**Developer:** Developer 1 (Module 1 of 4)  
**Target Commit Message:** `feat: implement student management`

---

## 1. What Module 1 Implements

Module 1 establishes the shared application foundation and delivers the complete **Student Management** system across `CRM-System.Core`, `CRM-System.Infrastructure`, `CRM-System.API`, `CRM-System.Web`, and `CRM-System.Tests`:

- **Authentication & Role-Based Security:**
  - Student self-registration (`/register`) and portal sign-in (`/login`) with session persistence and logout.
  - Password security powered by ASP.NET Core Identity's `PasswordHasher<ApplicationUser>` (PBKDF2-HMAC-SHA256; never stores plaintext passwords).
  - Role-based authorization (`Student` and `Admin` roles) enforced both in the UI and on the server side (`AccessContext` checks in `StudentService` and API endpoints).
  - Password update workflow (`/settings` and `POST /api/auth/change-password`).
- **Student Profiles:**
  - Full student entity & DTOs (`FullName`, `Email`, `StudentNumber`, `PhoneNumber`, `DateOfBirth`, `University`, `Faculty`, `Department`, `StudyProgram`, `StudyYear`, `Bio`, `Status`, `EnrollmentDate`).
  - Student self-service profile view and permitted personal profile editor (`/profile` and `PUT /api/students/{id}/profile`).
- **Administrator Student CRUD & Directory (`/students`):**
  - Search by name, email, student ID, department, or program.
  - Filter by faculty, study year (1–6), and status (`Active`, `Inactive`, `Graduated`, `Suspended`).
  - Create new student records with optional automatic portal login provisioning (`/students/create`).
  - View detailed student profiles (`/students/{id}`) and edit full administrative records (`/students/{id}/edit`).
  - Activate/deactivate student accounts (`SetStudentStatusAsync`) and delete records with confirmation modal (`DeleteStudentAsync`).
- **SQLite Persistence & EF Core Migrations:**
  - `CrmDbContext` configured with SQLite (`crm_students.db`), entity configurations (`Users`, `Students`), initial migration (`20261114000000_InitialStudentManagement`), and idempotent demo seeding (`DatabaseInitializer`).

---

## 2. Files Added, Modified, and Replaced

### Modified / Replaced Existing Files (8 files)
- `src/CRM-System.Infrastructure/CRM-System.Infrastructure.csproj` — Added EF Core SQLite (`10.0.0`) packages and ASP.NET Core shared framework reference.
- `src/CRM-System.API/Program.cs` — Configured SQLite infrastructure, cookie authentication, database migration/seeding on startup, OpenAPI, and mapped `/api/auth` and `/api/students` endpoints.
- `src/CRM-System.API/appsettings.json` — Added default SQLite connection string (`Data Source=crm_students.db`).
- `src/CRM-System.API/CRM-System.API.http` — Added ready-to-run HTTP test requests for authentication and student endpoints.
- `src/CRM-System.Web/CRM-System.Web.csproj` — Added project references to `CRM-System.Core` and `CRM-System.Infrastructure`.
- `src/CRM-System.Web/Program.cs` — Registered `CrmInfrastructure`, `CrmAuthStateProvider`, cascading authentication state, and startup database initialization.
- `src/CRM-System.Web/appsettings.json` — Added default SQLite connection string (`Data Source=crm_students.db`).
- `src/CRM-System.Web/Components/App.razor` — Enabled `InteractiveServer` render mode on `<Routes>` and `<HeadOutlet>`.
- `src/CRM-System.Web/Components/Routes.razor` — Wrapped router in `<CascadingAuthenticationState>`.
- `src/CRM-System.Web/Components/_Imports.razor` — Added global namespace imports for auth, shared components, Core models, DTOs, and interfaces.
- `src/CRM-System.Web/Components/Layout/MainLayout.razor` & `MainLayout.razor.css` — Implemented modern SaaS header, user identity badge, notification indicator, and sign-out controls.
- `src/CRM-System.Web/Components/Layout/NavMenu.razor` & `NavMenu.razor.css` — Implemented role-aware sidebar navigation covering all 11 CRM sections.
- `src/CRM-System.Web/Components/Pages/Home.razor` — Implemented Stage 1 role-aware overview dashboard and demo quick-login actions.
- `src/CRM-System.Web/wwwroot/app.css` — Implemented custom SaaS design system tokens, cards, tables, avatars, badges, and modal styles.
- `tests/CRM-System.Tests/CRM-System.Tests.csproj` — Added `Microsoft.EntityFrameworkCore.InMemory` and `Microsoft.EntityFrameworkCore.Sqlite` (`10.0.0`) for testing.

### Added Files (33 files)
See `DELIVERY/MANIFEST_STAGE_01.md` for the complete file-by-file list.

### Deleted Files
None. No manual file deletions are required.

---

## 3. Extraction & Application Instructions

1. Extract `01_Student_Management.zip` **directly into the root directory** of your local `CRM-System` repository (the folder containing `CRM-System.slnx` and `src/`).
2. Allow existing files to be overwritten when prompted.
3. Verify the changes with `git status`.

**PowerShell extraction example:**
```powershell
Expand-Archive -Path .\01_Student_Management.zip -DestinationPath . -Force
```

---

## 4. Database Migration, Build, and Test Commands

### Database Migrations
The application automatically applies pending EF Core migrations (`dbContext.Database.MigrateAsync()`) and seeds initial demo accounts on startup. If you wish to run EF CLI commands manually:
```powershell
dotnet ef database update --project src/CRM-System.Infrastructure --startup-project src/CRM-System.Web
```

### Build the Solution
```powershell
dotnet build CRM-System.slnx
```

### Run the Automated Test Suite
```powershell
dotnet test CRM-System.slnx
```

### Run the Applications
- **Blazor Web Portal:**
  ```powershell
  dotnet run --project src/CRM-System.Web
  ```
- **REST Web API:**
  ```powershell
  dotnet run --project src/CRM-System.API
  ```

---

## 5. How to Verify Module 1

1. Start `CRM-System.Web` and open the local URL (`http://localhost:5106`).
2. Test **Administrator Login**:
   - Email: `admin@crm.edu` | Password: `Admin123!` (or click the **Sign In as Administrator** quick demo button).
   - Navigate to **Students** (`/students`), search and filter students by faculty/year/status, create a new student (`/students/create`), edit an existing student (`/students/1/edit`), and toggle Active/Inactive status.
3. Test **Student Login & Ownership Protection**:
   - Sign out and sign in as `aidar.nurlanov@student.crm.edu` / `Student123!` (or register a brand-new student at `/register`).
   - Open **My Profile** (`/profile`) and update your personal bio, phone number, or study year.
   - Verify that navigating to `/students` or another student's profile (`/students/2`) is blocked by server-side authorization.
4. Commit Stage 1:
   ```powershell
   git add .
   git commit -m "feat: implement student management"
   git push origin main
   ```

---

## 6. Known Limitations at Stage 1

- Sidebar links for Stage 2 (`Courses`, `Academic Performance`, `Schedule`, `Attendance`) and Stage 3 (`Tasks`, `Calendar`, `Notifications`) render a clean Stage Roadmap page in Stage 1 so that all navigation links work without 404 errors; these routes are replaced with full implementations in Stages 2 and 3.
