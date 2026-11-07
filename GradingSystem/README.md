# Student Grade Management System

A Student Grade Management System built with **ASP.NET Core 8** (Web API + MVC), **Entity Framework Core**, **SQL Server**, **ASP.NET Core Identity**, and a **JavaScript `fetch()`** front end.

Faculty can manage grade records (create, read, update, delete). Students can sign in and view **only their own** grades. Grade **remarks are computed on the server**, never trusted from the browser.

---

## 1. Technology

- ASP.NET Core 8 Web API (REST endpoints under `/api`)
- ASP.NET Core 8 MVC (Razor views for the dashboards)
- Entity Framework Core 8 (SQL Server provider) with migrations
- ASP.NET Core Identity (cookie authentication, Faculty / Student roles)
- JavaScript `fetch()` with `async/await` and JSON
- Postman for API and security testing

## 2. Prerequisites

- Windows + Visual Studio 2022 (or the .NET 8 SDK)
- SQL Server reachable at `localhost` (SQL Server Express / Developer / LocalDB) using Windows authentication
- Postman

## 3. Configuration

Settings live in `appsettings.json`:

- **Connection string** (`ConnectionStrings:Default`):
  `Server=localhost;Database=StudentGradeManagementDB;Trusted_Connection=True;TrustServerCertificate=True`
  Change `Server=` if your SQL Server instance name is different (e.g. `Server=localhost\SQLEXPRESS`).
- **Seed password** (`Seed:DemoPassword`): `Test@12345` — the password given to every seeded demo account.

The HTTPS URL is `https://localhost:7173` (see `Properties/launchSettings.json`, profile **https**).

## 4. Database setup

This project uses **EF Core migrations** and seeds data automatically on startup
(`Program.cs` calls `DbSeeder.SeedAsync`, which runs `Database.MigrateAsync()` first).

**Option A (recommended): just run the app.** On first launch it creates the
`StudentGradeManagementDB` database, applies the migration, and seeds roles,
students, faculty, sample grades, and login accounts.

If you prefer to create the database before running:
- Package Manager Console: `Update-Database`
- or CLI: `dotnet ef database update`

**Option B: the instructor SQL script** (`StudentGradeManagementDB_WS_FINAL_PROJECT.sql`)
creates the same academic tables in SQL Server Management Studio. Use **either** the EF
migration **or** the SQL script against a database — do not mix both on the same database.
Login accounts are always created by Identity in application code, not by the SQL script.

## 5. Running the app

1. Open `GradingSystem.slnx` in Visual Studio.
2. Make sure the **https** launch profile is selected.
3. Press **F5**. The browser opens at `https://localhost:7173`.
4. There is no login link on the Home page by design — go to
   `https://localhost:7173/Account/Login` (or just open `/Faculty` or `/Student`,
   which redirect you to the login page).

## 6. Demo accounts (password `Test@12345`)

| Role | Email | Lands on |
|------|-------|----------|
| Faculty | andrea.garcia@example.com | /Faculty |
| Faculty | michael.ramos@example.com | /Faculty |
| Student | juan.delacruz@example.com (2026-0001) | /Student |
| Student | maria.santos@example.com (2026-0002) | /Student |
| Student | pedro.reyes@example.com (2026-0003) | /Student |

> Demo credentials only — do not use real personal passwords.

## 7. How the dashboards work

- **Faculty dashboard** (`/Faculty`, `Views/Faculty/Index.cshtml`): loads all grades with
  `fetch('/api/grades')`, renders a table, and supports **Add** (`POST`), **Edit**
  (`PUT /api/grades/{id}`), and **Delete** (`DELETE` after a `confirm()`). Failed requests
  show a message instead of crashing. There is no Remarks field — the server sets it.
- **Student dashboard** (`/Student`, `Views/Student/Index.cshtml`): loads
  `fetch('/api/student/grades')`, shows the signed-in student's name and number and their
  grades. No create/edit/delete controls. The student is identified from the **login
  account**, not from any value in the request.

## 8. API endpoints

| Method | Route | Allowed | Success |
|--------|-------|---------|---------|
| POST | `/api/auth/login` | anyone | 200 |
| POST | `/api/auth/logout` | authenticated | 204 |
| GET | `/api/grades` | Faculty | 200 |
| GET | `/api/grades/{id}` | Faculty, or the owning Student | 200 (404 if not owner) |
| POST | `/api/grades` | Faculty | 201 |
| PUT | `/api/grades/{id}` | Faculty | 200 |
| DELETE | `/api/grades/{id}` | Faculty | 204 |
| GET | `/api/student/grades` | Student | 200 |

## 9. Grade remarks (computed on the server)

| Whole-number grade | Remark |
|--------------------|--------|
| 95–100 | CMS |
| 90–94 | SP |
| 85–89 | SAP |
| 80–84 | GP |
| 75–79 | EP |
| 0–74 | BEP/FAILED |

Implemented in `Services/GradeRemarks.For(int grade)` and called by the API on every
create and update. Any `Remarks` value sent by a client is ignored (the request model
has no Remarks property).

## 10. Validation and data integrity

- Grade must be a whole number 0–100 (`[Required, Range(0, 100)]` on an `int?`).
- `105` and `-1` → **400** (range); `85.5` or text → **400** (cannot bind to `int`); missing grade → **400**.
- Duplicate grade for the same student + subject + semester + school year → **409** (also enforced by a DB unique constraint).
- Grade range is enforced in both API validation and the database (`CHECK (Grade BETWEEN 0 AND 100)`).
- All data access uses EF Core (parameterized) — no string-concatenated SQL.

## 11. Authentication, authorization, ownership

- Identity cookie authentication; `ApplicationUser` has nullable `StudentId` / `FacultyId` mapping each login to an academic record.
- API requests return **401** when not signed in and **403** when signed in without the required role (configured in `Program.cs`).
- `GET /api/grades/{id}` returns **404** when a student requests a grade they do not own, concealing other students' records.
- `GET /api/student/grades` derives the student from the authenticated account, so changing a URL or body cannot expose another student's grades.

## 12. Postman testing

Import `GradingSystem.postman_collection.json`. Run the requests top to bottom:

1. **Faculty** folder — login, GET, POST (valid), PUT (remark change), DELETE, and the
   invalid-grade tests (105, -1, 85.5) plus the fake-Remarks test.
2. **Student** folder — login, own grades, ownership test (`GET /api/grades/3` → 404),
   and write-restriction tests (POST/PUT/DELETE → 403).
3. **No auth** folder — logout, then a call with no session → 401.

Capture the method, endpoint, role, status, and response body for each (screenshots).

## 13. Project structure (key files)

```
Controllers/Api/AuthApiController.cs      login / logout
Controllers/Api/GradesController.cs       faculty CRUD + ownership-aware GET
Controllers/Api/StudentGradesController.cs student's own grades
Controllers/AccountController.cs          MVC login/logout, role-based redirect
Controllers/FacultyController.cs          serves /Faculty (Faculty role)
Controllers/StudentController.cs          serves /Student (Student role)
Models/StudentGrade.cs, GradeRequest.cs   entity + request DTO
Services/GradeRemarks.cs                  server-side remark computation
Data/AppDbContext.cs, DbSeeder.cs         EF context + seed data
Views/Faculty/Index.cshtml                faculty dashboard (fetch CRUD)
Views/Student/Index.cshtml                student dashboard (fetch read-only)
Program.cs                                Identity, 401/403 for /api, seeding
```
