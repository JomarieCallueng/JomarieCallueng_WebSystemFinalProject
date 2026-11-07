using System.ComponentModel.DataAnnotations;

namespace GradingSystem.Models;

public class GradeRequest
{
    [Required]
    public int? StudentId { get; set; }

    [Required, StringLength(30)]
    public string SubjectCode { get; set; } = "";

    [Required, StringLength(200)]
    public string SubjectName { get; set; } = "";

    // int? so a missing grade is caught; Range blocks below 0 and above 100.
    // A decimal like 85.5 or text like "abc" fails automatically with 400.
    [Required, Range(0, 100)]
    public int? Grade { get; set; }

    [Required, StringLength(50)]
    public string Semester { get; set; } = "";

    [Required, StringLength(20)]
    public string SchoolYear { get; set; } = "";
}