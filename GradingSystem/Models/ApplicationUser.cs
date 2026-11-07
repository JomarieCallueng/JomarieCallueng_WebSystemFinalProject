using Microsoft.AspNetCore.Identity;

namespace GradingSystem.Models;

public class ApplicationUser : IdentityUser
{
    public int? StudentId { get; set; }
    public Student? Student { get; set; }
    public int? FacultyId { get; set; }
    public Faculty? Faculty { get; set; }
}