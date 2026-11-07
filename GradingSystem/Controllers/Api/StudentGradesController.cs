using GradingSystem.Data;
using GradingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Controllers.Api;

[ApiController]
[Route("api/student/grades")]
[Authorize(Roles = "Student")]
public class StudentGradesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public StudentGradesController(AppDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    // GET /api/student/grades
    // The student is identified from the login account, never from the request.
    [HttpGet]
    public async Task<IActionResult> Mine()
    {
        var me = await _users.GetUserAsync(User);
        if (me?.StudentId == null) return Forbid();

        var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentId == me.StudentId);
        if (student == null) return NotFound();

        var grades = await _db.StudentGrades
            .Where(g => g.StudentId == me.StudentId)
            .OrderBy(g => g.SubjectCode)
            .Select(g => new
            {
                g.GradeId,
                g.SubjectCode,
                g.SubjectName,
                g.Grade,
                g.Remarks,
                g.Semester,
                g.SchoolYear
            })
            .ToListAsync();

        return Ok(new
        {
            studentNumber = student.StudentNumber,
            studentName = student.FirstName + " " + student.LastName,
            grades
        });
    }
}