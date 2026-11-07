using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GradingSystem.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    public IActionResult Index() => View();
}