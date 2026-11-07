using GradingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GradingSystem.Controllers.Api;

public record LoginDto(string Email, string Password);

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _users;

    public AuthApiController(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users)
    {
        _signIn = signIn;
        _users = users;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto d)
    {
        var r = await _signIn.PasswordSignInAsync(d.Email, d.Password, false, false);
        if (!r.Succeeded) return Unauthorized(new { message = "Invalid email or password." });

        var user = await _users.FindByEmailAsync(d.Email);
        var roles = await _users.GetRolesAsync(user!);
        return Ok(new { email = user!.Email, roles });
    }

    [HttpPost("logout"), Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return NoContent();
    }
}