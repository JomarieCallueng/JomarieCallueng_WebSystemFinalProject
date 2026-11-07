using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GradingSystem.Controllers;

[Authorize(Roles = "Faculty")]
public class FacultyController : Controller
{
    public IActionResult Index() => View();
}