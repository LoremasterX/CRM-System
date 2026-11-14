# Stage 1 File Manifest (`01_Student_Management.zip`)

| Relative File Path | Operation | Short Description |
| :--- | :--- | :--- |
| `src/CRM-System.Core/Common/ServiceResult.cs` | Added | Standardized operation result wrapper and error categorization |
| `src/CRM-System.Core/Common/AccessContext.cs` | Added | Server-side caller identity, role, and student ownership context |
| `src/CRM-System.Core/Students/Models/UserRoles.cs` | Added | `Admin` and `Student` role constants and normalization helpers |
| `src/CRM-System.Core/Students/Models/StudentStatus.cs` | Added | Enumeration of student lifecycle statuses (`Active`, `Inactive`, `Graduated`, `Suspended`) |
| `src/CRM-System.Core/Students/Models/ApplicationUser.cs` | Added | Authentication user entity with PBKDF2 password hash and role |
| `src/CRM-System.Core/Students/Models/Student.cs` | Added | Student domain entity with academic metadata and profile attributes |
| `src/CRM-System.Core/Students/DTOs/AuthDtos.cs` | Added | Login, student registration, password change, and session DTOs |
| `src/CRM-System.Core/Students/DTOs/StudentDtos.cs` | Added | Student CRUD, personal profile update, filter, and directory stats DTOs |
| `src/CRM-System.Core/Students/Validation/StudentValidator.cs` | Added | Server-side validation rules for auth and student operations |
| `src/CRM-System.Core/Students/Interfaces/IPasswordHasher.cs` | Added | Password hashing and verification abstraction |
| `src/CRM-System.Core/Students/Interfaces/IUserRepository.cs` | Added | User persistence contract |
| `src/CRM-System.Core/Students/Interfaces/IStudentRepository.cs` | Added | Student CRUD, search, filtering, and ID generation contract |
| `src/CRM-System.Core/Students/Interfaces/IAuthService.cs` | Added | Authentication and registration service contract |
| `src/CRM-System.Core/Students/Interfaces/IStudentService.cs` | Added | Student management and profile service contract |
| `src/CRM-System.Core/Students/Services/AuthService.cs` | Added | Authentication, registration, and password management business logic |
| `src/CRM-System.Core/Students/Services/StudentService.cs` | Added | Role- and ownership-protected student CRUD and profile business logic |
| `src/CRM-System.Infrastructure/CRM-System.Infrastructure.csproj` | Modified | Added EF Core SQLite 10.0.0 packages and ASP.NET Core framework reference |
| `src/CRM-System.Infrastructure/DependencyInjection.cs` | Added | Shared DI registration (`AddCrmInfrastructure`) for API and Web |
| `src/CRM-System.Infrastructure/Data/CrmDbContext.cs` | Added | EF Core database context for `Users` and `Students` |
| `src/CRM-System.Infrastructure/Data/DatabaseInitializer.cs` | Added | Startup migration runner and idempotent demo data seeder |
| `src/CRM-System.Infrastructure/Data/Configurations/ApplicationUserConfiguration.cs` | Added | EF Core entity configuration and unique indexes for `Users` |
| `src/CRM-System.Infrastructure/Data/Configurations/StudentConfiguration.cs` | Added | EF Core entity configuration, indexes, and FK relationship for `Students` |
| `src/CRM-System.Infrastructure/Migrations/20261114000000_InitialStudentManagement.cs` | Added | Initial EF Core migration creating `Users` and `Students` tables |
| `src/CRM-System.Infrastructure/Migrations/20261114000000_InitialStudentManagement.Designer.cs` | Added | EF Core migration designer metadata |
| `src/CRM-System.Infrastructure/Migrations/CrmDbContextModelSnapshot.cs` | Added | EF Core cumulative model snapshot (Stage 1) |
| `src/CRM-System.Infrastructure/Students/PasswordHasherAdapter.cs` | Added | ASP.NET Core Identity `PasswordHasher<ApplicationUser>` implementation |
| `src/CRM-System.Infrastructure/Students/UserRepository.cs` | Added | EF Core repository for `ApplicationUser` |
| `src/CRM-System.Infrastructure/Students/StudentRepository.cs` | Added | EF Core repository for `Student` with search, filter, and sorting |
| `src/CRM-System.API/Program.cs` | Modified | Configured SQLite, Cookie Auth, OpenAPI, and mapped Stage 1 API endpoints |
| `src/CRM-System.API/appsettings.json` | Modified | Added SQLite `DefaultConnection` string |
| `src/CRM-System.API/CRM-System.API.http` | Modified | Added sample HTTP requests for Auth and Student REST endpoints |
| `src/CRM-System.API/Endpoints/StudentEndpoints.cs` | Added | REST API endpoints for `/api/auth/*` and `/api/students/*` |
| `src/CRM-System.Web/CRM-System.Web.csproj` | Modified | Added project references to `Core` and `Infrastructure` |
| `src/CRM-System.Web/Program.cs` | Modified | Configured DI, auth state provider, and database initialization |
| `src/CRM-System.Web/appsettings.json` | Modified | Added SQLite `DefaultConnection` string |
| `src/CRM-System.Web/Auth/CrmAuthStateProvider.cs` | Added | Blazor Server authentication state provider with browser storage persistence |
| `src/CRM-System.Web/Components/App.razor` | Modified | Configured `InteractiveServer` render mode |
| `src/CRM-System.Web/Components/Routes.razor` | Modified | Added `<CascadingAuthenticationState>` wrapper |
| `src/CRM-System.Web/Components/_Imports.razor` | Modified | Added Stage 1 namespaces and authorization imports |
| `src/CRM-System.Web/Components/Layout/MainLayout.razor` | Modified | SaaS top navigation bar, user profile badge, and sign-out controls |
| `src/CRM-System.Web/Components/Layout/MainLayout.razor.css` | Modified | Updated layout and top bar styling |
| `src/CRM-System.Web/Components/Layout/NavMenu.razor` | Modified | Role-aware sidebar navigation with all 11 CRM sections |
| `src/CRM-System.Web/Components/Layout/NavMenu.razor.css` | Modified | Updated sidebar navigation styling |
| `src/CRM-System.Web/Components/Shared/StatusBadge.razor` | Added | Reusable student status badge component |
| `src/CRM-System.Web/Components/Shared/StatCard.razor` | Added | Reusable KPI metric card component |
| `src/CRM-System.Web/Components/Shared/ConfirmDialog.razor` | Added | Reusable modal confirmation dialog component |
| `src/CRM-System.Web/Components/Shared/EmptyStateCard.razor` | Added | Reusable empty/restricted state card component |
| `src/CRM-System.Web/Components/Pages/Home.razor` | Modified | Role-aware overview dashboard and quick demo sign-in |
| `src/CRM-System.Web/Components/Pages/Students/Login.razor` | Added | Sign-in page with validation and one-click demo credentials |
| `src/CRM-System.Web/Components/Pages/Students/Register.razor` | Added | Student self-registration page |
| `src/CRM-System.Web/Components/Pages/Students/MyProfile.razor` | Added | Student personal profile viewer and self-service editor |
| `src/CRM-System.Web/Components/Pages/Students/StudentList.razor` | Added | Administrator student directory with search, filter, status toggle, and delete |
| `src/CRM-System.Web/Components/Pages/Students/StudentCreate.razor` | Added | Administrator create student page |
| `src/CRM-System.Web/Components/Pages/Students/StudentDetails.razor` | Added | Student record details page with ownership check |
| `src/CRM-System.Web/Components/Pages/Students/StudentEdit.razor` | Added | Administrator edit student record page |
| `src/CRM-System.Web/Components/Pages/Students/Settings.razor` | Added | Account security and password update page |
| `src/CRM-System.Web/Components/Pages/Students/UpcomingStageRoutes.razor` | Added | Stage roadmap view for Stage 2–3 sidebar links |
| `src/CRM-System.Web/wwwroot/app.css` | Modified | Modern SaaS Student CRM design system styles |
| `tests/CRM-System.Tests/CRM-System.Tests.csproj` | Modified | Added EF Core InMemory and Sqlite package references |
| `tests/CRM-System.Tests/Students/AuthServiceTests.cs` | Added | Unit & integration tests for registration, hashing, login, and password change |
| `tests/CRM-System.Tests/Students/StudentServiceTests.cs` | Added | Unit & integration tests for CRUD, search/filter, and role/ownership security |
| `tests/CRM-System.Tests/Students/StudentValidatorTests.cs` | Added | Validation contract tests for invalid inputs |
| `tests/CRM-System.Tests/Students/DatabaseInitializerTests.cs` | Added | SQLite relational migration and idempotent demo seeding tests |
| `DELIVERY/README_STAGE_01.md` | Added | Stage 1 setup, build, test, and verification guide |
| `DELIVERY/MANIFEST_STAGE_01.md` | Added | Stage 1 file manifest |
