# Live Demo Prep — Student Grade Management System

Demo accounts (password `Test@12345`): faculty `andrea.garcia@example.com`,
student `juan.delacruz@example.com`. App runs at `https://localhost:7173`.

## Suggested demo flow (10 minutes)

1. Run the app (F5). Open `/Account/Login`, sign in as **faculty**.
2. Faculty dashboard: show the table loads via `fetch`. **Add** a grade, **Edit** it (show the remark change), **Delete** it (show the `confirm()` box and the error-message area).
3. Sign out, sign in as **student** (`juan.delacruz`). Show own grades, no edit/delete controls.
4. Switch to Postman. Run the collection folders: Faculty, Student, No authentication. Point at the green test checks and the status codes.
5. Be ready to answer the questions below.

---

## Q&A — the four things you must be able to explain

### A. Authentication flow
**Q: How does login work?**
Identity with cookie authentication. `POST /api/auth/login` calls
`SignInManager.PasswordSignInAsync`; on success the server issues an auth cookie the
browser stores. Every later request sends that cookie, so the API knows who you are.
The MVC `AccountController` login does the same and then redirects Faculty to `/Faculty`
and Student to `/Student` based on their role.

**Q: Why 401 for the API instead of a redirect to the login page?**
In `Program.cs` I override `OnRedirectToLogin` / `OnRedirectToAccessDenied`: if the path
starts with `/api`, I return **401** (not signed in) or **403** (signed in, wrong role)
instead of an HTML redirect. APIs should return status codes, not login pages.

**Q: How is a login linked to a student or faculty record?**
`ApplicationUser` extends IdentityUser with nullable `StudentId` and `FacultyId`. The
seeder sets these when it creates each account, so an account maps to exactly one academic
record.

### B. Ownership / data protection
**Q: How do you stop a student seeing another student's grades?**
Two layers. `GET /api/student/grades` ignores anything in the request and reads
`StudentId` from the logged-in account (`UserManager.GetUserAsync`), returning only that
student's rows. And `GET /api/grades/{id}`: if the caller is a Student and the grade's
`StudentId` is not theirs, it returns **404** — concealing that the record exists.

**Q: Demonstrate it.**
Signed in as Juan, `GET /api/grades/3` (Maria's grade) returns 404. Changing the id in the
URL gets you nothing.

**Q: Why 404 and not 403 there?**
403 would confirm the record exists. 404 hides its existence — the exam explicitly allows
this. (Write attempts by a student still return 403, because that's a role problem, not a
hidden record.)

### C. Validation
**Q: How are grades validated?**
`GradeRequest.Grade` is `int?` with `[Required, Range(0, 100)]`. Because the controller is
`[ApiController]`, an invalid model auto-returns **400** before my code runs.
- `105` and `-1` fail the Range rule.
- `85.5` fails JSON binding — it can't convert to `int` — also 400.
- A missing grade fails Required — 400.
The database also has `CHECK (Grade BETWEEN 0 AND 100)`, so the rule holds even outside the API.

**Q: What about duplicate grades?**
Create/update check for an existing row with the same student + subject + semester + school
year and return **409 Conflict**; a DB unique constraint enforces the same.

### D. Remark computation
**Q: Where do remarks come from?**
`Services/GradeRemarks.For(int grade)` — a `switch` mapping 95+ to CMS, 90+ SP, 85+ SAP,
80+ GP, 75+ EP, else BEP/FAILED. Create and update both set `Remarks = GradeRemarks.For(...)`.

**Q: What if the client sends its own remark?**
It's ignored. `GradeRequest` has no `Remarks` property, so a `"remarks"` field in the JSON
has nowhere to bind. Postman test 09 proves it: sending `grade 88` with `remarks "CMS"`
returns `remarks "SAP"`.

**Q: Does updating a grade leave the old remark?**
No — `PUT` recomputes the remark from the new number every time. Postman test 04 shows a
92/SP record become 97/CMS.

---

## Likely extra questions

- **Why `fetch` with `async/await`?** It calls the API asynchronously without reloading the page, then parses JSON and builds the table. Every call checks `res.ok` and shows a message on failure, so a bad response never crashes the page.
- **Is hiding the Edit/Delete buttons your security?** No. The buttons only exist on the faculty page, but the real protection is `[Authorize(Roles = "Faculty")]` on the API. A student hitting the API directly still gets 403.
- **Where is data stored / does it survive restart?** SQL Server (`StudentGradeManagementDB`) via EF Core. Records persist across restarts; the seeder only inserts sample grades when the table is empty.
- **Any raw SQL / injection risk?** No — all access is through EF Core, which parameterizes queries.
- **What does CMS/SP/etc. stand for?** They are the institution's remark codes for each grade band (as given in the exam's grading scale).
