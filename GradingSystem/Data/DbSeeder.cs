using GradingSystem.Models;
using GradingSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider sp, string demoPassword)
    {
        var db = sp.GetRequiredService<AppDbContext>();
        var roles = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var users = sp.GetRequiredService<UserManager<ApplicationUser>>();

        await db.Database.MigrateAsync();

        // 1. Roles
        foreach (var r in new[] { "Faculty", "Student" })
            if (!await roles.RoleExistsAsync(r))
                await roles.CreateAsync(new IdentityRole(r));

        // 2. Students (each added only if missing)
        var studentSeed = new[]
        {
            ("2026-0001", "Juan",  "Dela Cruz", "juan.delacruz@example.com"),
            ("2026-0002", "Maria", "Santos",    "maria.santos@example.com"),
            ("2026-0003", "Pedro", "Reyes",     "pedro.reyes@example.com"),
        };
        foreach (var (no, fn, ln, em) in studentSeed)
            if (!await db.Students.AnyAsync(s => s.StudentNumber == no))
                db.Students.Add(new Student { StudentNumber = no, FirstName = fn, LastName = ln, Email = em });

        // 3. Faculty
        var facultySeed = new[]
        {
            ("FAC-001", "Andrea",  "Garcia", "andrea.garcia@example.com"),
            ("FAC-002", "Michael", "Ramos",  "michael.ramos@example.com"),
        };
        foreach (var (no, fn, ln, em) in facultySeed)
            if (!await db.Faculty.AnyAsync(f => f.FacultyNumber == no))
                db.Faculty.Add(new Faculty { FacultyNumber = no, FirstName = fn, LastName = ln, Email = em });

        await db.SaveChangesAsync();

        // 4. Sample grades (only when the table is empty)
        if (!await db.StudentGrades.AnyAsync())
        {
            var fac1 = await db.Faculty.FirstAsync(f => f.FacultyNumber == "FAC-001");
            var gradeSeed = new[]
            {
                ("2026-0001", "WEB101", "Web Systems and Technologies", 98),
                ("2026-0001", "NET101", "Networking Fundamentals", 92),
                ("2026-0002", "WEB101", "Web Systems and Technologies", 87),
                ("2026-0002", "NET101", "Networking Fundamentals", 82),
                ("2026-0003", "WEB101", "Web Systems and Technologies", 76),
                ("2026-0003", "NET101", "Networking Fundamentals", 68),
            };
            foreach (var (sno, code, name, grade) in gradeSeed)
            {
                var stu = await db.Students.FirstAsync(s => s.StudentNumber == sno);
                db.StudentGrades.Add(new StudentGrade
                {
                    StudentId = stu.StudentId,
                    SubjectCode = code,
                    SubjectName = name,
                    FacultyId = fac1.FacultyId,
                    Grade = grade,
                    Remarks = GradeRemarks.For(grade),
                    Semester = "1st Semester",
                    SchoolYear = "2026-2027"
                });
            }
            await db.SaveChangesAsync();
        }

        // 5. Login accounts linked to the academic records
        foreach (var s in await db.Students.ToListAsync())
        {
            if (await users.FindByEmailAsync(s.Email) != null) continue;
            var u = new ApplicationUser
            {
                UserName = s.Email,
                Email = s.Email,
                EmailConfirmed = true,
                StudentId = s.StudentId
            };
            var res = await users.CreateAsync(u, demoPassword);
            if (res.Succeeded) await users.AddToRoleAsync(u, "Student");
        }

        foreach (var f in await db.Faculty.ToListAsync())
        {
            if (await users.FindByEmailAsync(f.Email) != null) continue;
            var u = new ApplicationUser
            {
                UserName = f.Email,
                Email = f.Email,
                EmailConfirmed = true,
                FacultyId = f.FacultyId
            };
            var res = await users.CreateAsync(u, demoPassword);
            if (res.Succeeded) await users.AddToRoleAsync(u, "Faculty");
        }
    }
}