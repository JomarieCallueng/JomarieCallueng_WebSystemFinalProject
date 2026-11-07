namespace GradingSystem.Models;

public class Faculty
{
    public int FacultyId { get; set; }
    public string FacultyNumber { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsActive { get; set; } = true;
}