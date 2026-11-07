namespace GradingSystem.Models;

public class StudentGrade
{
    public int GradeId { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public string SubjectCode { get; set; } = "";
    public string SubjectName { get; set; } = "";
    public int FacultyId { get; set; }
    public Faculty? Faculty { get; set; }
    public int Grade { get; set; }
    public string Remarks { get; set; } = "";
    public string Semester { get; set; } = "";
    public string SchoolYear { get; set; } = "";
    public DateTime DatePosted { get; set; } = DateTime.UtcNow;
    public DateTime? DateModified { get; set; }
}