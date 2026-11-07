using GradingSystem.Data;
using GradingSystem.Models;
using GradingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Controllers.Api;

[ApiController]
[Route("api/grades")]
public class GradesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public GradesController(AppDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    private static object ToDto(StudentGrade g) => new
    {
        g.GradeId,
        g.StudentId,
        StudentNumber = g.Student!.StudentNumber,
        StudentName = g.Student.FirstName + " " + g.Student.LastName,
        g.SubjectCode,
        g.SubjectName,
        g.Grade,
        g.Remarks,
        g.Semester,
        g.SchoolYear,
        g.DatePosted,
        g.DateModified
    };

    // GET /api/grades  (Faculty only)
    [HttpGet, Authorize(Roles = "Faculty")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.StudentGrades
            .Include(g => g.Student)
            .OrderBy(g => g.Student!.StudentNumber).ThenBy(g => g.SubjectCode)
            .ToListAsync();
        return Ok(list.Select(ToDto));
    }

    // GET /api/grades/5  (Faculty, or a Student who owns the grade)
    [HttpGet("{id:int}"), Authorize]
    public async Task<IActionResult> Get(int id)
    {
        var g = await _db.StudentGrades.Include(x => x.Student)
            .FirstOrDefaultAsync(x => x.GradeId == id);
        if (g == null) return NotFound();

        if (User.IsInRole("Student"))
        {
            var me = await _users.GetUserAsync(User);
            if (me?.StudentId != g.StudentId) return NotFound(); // hides other students' records
        }
        return Ok(ToDto(g));
    }

    // POST /api/grades  (Faculty only)
    [HttpPost, Authorize(Roles = "Faculty")]
    public async Task<IActionResult> Create(GradeRequest r)
    {
        var me = await _users.GetUserAsync(User);
        if (me?.FacultyId == null) return Forbid();

        if (!await _db.Students.AnyAsync(s => s.StudentId == r.StudentId))
            return BadRequest(new { message = "Student not found." });

        var duplicate = await _db.StudentGrades.AnyAsync(g =>
            g.StudentId == r.StudentId && g.SubjectCode == r.SubjectCode &&
            g.Semester == r.Semester && g.SchoolYear == r.SchoolYear);
        if (duplicate)
            return Conflict(new { message = "A grade already exists for this student, subject, semester and school year." });

        var grade = new StudentGrade
        {
            StudentId = r.StudentId!.Value,
            SubjectCode = r.SubjectCode,
            SubjectName = r.SubjectName,
            FacultyId = me.FacultyId.Value,
            Grade = r.Grade!.Value,
            Remarks = GradeRemarks.For(r.Grade.Value),   // computed by the server
            Semester = r.Semester,
            SchoolYear = r.SchoolYear
        };
        _db.StudentGrades.Add(grade);
        await _db.SaveChangesAsync();
        await _db.Entry(grade).Reference(x => x.Student).LoadAsync();

        return CreatedAtAction(nameof(Get), new { id = grade.GradeId }, ToDto(grade));
    }

    // PUT /api/grades/5  (Faculty only)
    [HttpPut("{id:int}"), Authorize(Roles = "Faculty")]
    public async Task<IActionResult> Update(int id, GradeRequest r)
    {
        var g = await _db.StudentGrades.Include(x => x.Student)
            .FirstOrDefaultAsync(x => x.GradeId == id);
        if (g == null) return NotFound();

        var duplicate = await _db.StudentGrades.AnyAsync(x =>
            x.GradeId != id && x.StudentId == g.StudentId && x.SubjectCode == r.SubjectCode &&
            x.Semester == r.Semester && x.SchoolYear == r.SchoolYear);
        if (duplicate)
            return Conflict(new { message = "A grade already exists for this student, subject, semester and school year." });

        g.SubjectCode = r.SubjectCode;
        g.SubjectName = r.SubjectName;
        g.Semester = r.Semester;
        g.SchoolYear = r.SchoolYear;
        g.Grade = r.Grade!.Value;
        g.Remarks = GradeRemarks.For(g.Grade);   // always recomputed, never the old remark
        g.DateModified = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(ToDto(g));
    }

    // DELETE /api/grades/5  (Faculty only)
    [HttpDelete("{id:int}"), Authorize(Roles = "Faculty")]
    public async Task<IActionResult> Delete(int id)
    {
        var g = await _db.StudentGrades.FindAsync(id);
        if (g == null) return NotFound();

        _db.StudentGrades.Remove(g);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}