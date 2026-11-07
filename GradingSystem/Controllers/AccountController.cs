using GradingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GradingSystem.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _users;

    public AccountController(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users)
    {
        _signIn = signIn;
        _users = users;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel m)
    {
        if (!ModelState.IsValid) return View(m);

        var result = await _signIn.PasswordSignInAsync(m.Email, m.Password, false, false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(m);
        }

        var user = await _users.FindByEmailAsync(m.Email);
        if (await _users.IsInRoleAsync(user!, "Faculty")) return RedirectToAction("Index", "Faculty");
        if (await _users.IsInRoleAsync(user!, "Student")) return RedirectToAction("Index", "Student");
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Login");
    }

    public IActionResult AccessDenied() => View();
}