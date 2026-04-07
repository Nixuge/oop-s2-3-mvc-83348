# Acme Global College (VGC) Student & Course Management System

This is an ASP.NET Core MVC application for managing courses, students, and faculty across multiple branches of Acme Global College.

## Core Features
- **Student Management:** Profiles, registration, and enrolment.
- **Academic Tracking:** Attendance, assignment results (gradebook), and exam results.
- **RBAC (Role-Based Access Control):** Admin, Faculty, and Student roles with specific visibility rules.
- **Result Visibility:** Students cannot see provisional exam results until they are released by an administrator.
- **Faculty Restrictions:** Faculty can only see courses and students assigned to them.

## Setup & Running Locally

1. **Clone the repository.**
2. **Navigate to the web project:**
   ```bash
   cd VgcCollege.Web
   ```
3. **Run the application:**
   ```bash
   dotnet run
   ```
   The database will be automatically created and seeded on the first run using SQLite (`vgccollege.db`).

## Running Tests

To run the xUnit unit tests:
```bash
dotnet test
```

## Seeded Demo Accounts

Use these credentials to test different roles:

| Role | Email | Password |
|------|-------|----------|
| **Administrator** | `admin@nixuge.me` | `Securite2026!` |
| **Faculty** | `faculty@nixuge.me` | `Securite2026!` |
| **Student 1** | `student1@nixuge.me` | `Securite2026!` |
| **Student 2** | `student2@nixuge.me` | `Securite2026!` |

## CI Workflow
The project includes a GitHub Actions workflow in `.github/workflows/ci.yml` that performs:
- Restoration of dependencies
- Release build
- Execution of unit tests

## Design Decisions
- **Project Structure:** 
  - `VgcCollege.Domain`: Shared class library for entities.
  - `VgcCollege.Web`: ASP.NET Core MVC project with Identity and EF Core.
  - `VgcCollege.Tests`: xUnit project for unit testing.
- **Persistence:** Uses **SQLite** for easy portability and setup.
- **Identity:** Uses **ASP.NET Core Identity** for authentication and RBAC.
- **Seeding:** Automatically populates 3 branches, 11 courses, and relevant profiles and results on startup via `DbInitializer`.
- **Visibility Constraint:** Implemented via server-side LINQ filtering in `StudentController` to ensure students only see `ResultsReleased == true` for exams.
