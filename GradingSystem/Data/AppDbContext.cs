using GradingSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Faculty> Faculty => Set<Faculty>();
    public DbSet<StudentGrade> StudentGrades => Set<StudentGrade>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Student>(e =>
        {
            e.HasKey(s => s.StudentId);
            e.HasIndex(s => s.StudentNumber).IsUnique();
            e.HasIndex(s => s.Email).IsUnique();
        });

        b.Entity<Faculty>(e =>
        {
            e.HasKey(f => f.FacultyId);
            e.HasIndex(f => f.FacultyNumber).IsUnique();
            e.HasIndex(f => f.Email).IsUnique();
        });

        b.Entity<StudentGrade>(e =>
        {
            e.HasKey(g => g.GradeId);   // 👈 this was missing

            e.ToTable("StudentGrades", t =>
                t.HasCheckConstraint("CK_StudentGrades_Grade", "[Grade] BETWEEN 0 AND 100"));

            e.HasIndex(g => new { g.StudentId, g.SubjectCode, g.Semester, g.SchoolYear }).IsUnique();

            e.HasOne(g => g.Student).WithMany().HasForeignKey(g => g.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(g => g.Faculty).WithMany().HasForeignKey(g => g.FacultyId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}