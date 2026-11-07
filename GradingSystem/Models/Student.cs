namespace GradingSystem.Models;

public class Student
{
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsActive { get; set; } = true;
}