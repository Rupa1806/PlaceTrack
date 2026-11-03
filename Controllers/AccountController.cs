using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlaceTrack.Data;
using PlaceTrack.Models;
using PlaceTrack.Services;
using PlaceTrack.ViewModels;

namespace PlaceTrack.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    public AccountController(AppDbContext db) => _db = db;

    // ---------- LOGIN ----------
    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
            return RedirectToDashboard();

        return View(new LoginVM { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginVM model)
    {
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim().ToLower();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Same message for "no such user" and "wrong password" (do not help attackers).
        if (user == null || !PasswordService.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Wrong email or password. Please try again.");
            return View(model);
        }

        await SignInUser(user);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToDashboard(user.Role);
    }

    // ---------- REGISTER (students only) ----------
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
            return RedirectToDashboard();

        return View(new RegisterVM());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVM model)
    {
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim().ToLower();
        var roll = model.RollNo.Trim().ToUpper();

        if (await _db.Users.AnyAsync(u => u.Email == email))
            ModelState.AddModelError(nameof(model.Email), "This email is already registered.");

        if (await _db.StudentProfiles.AnyAsync(s => s.RollNo == roll))
            ModelState.AddModelError(nameof(model.RollNo), "This roll number is already registered.");

        if (!Branches.All.Contains(model.Branch))
            ModelState.AddModelError(nameof(model.Branch), "Pick a branch from the list.");

        if (!ModelState.IsValid) return View(model);

        var user = new User
        {
            FullName = model.FullName.Trim(),
            Email = email,
            PasswordHash = PasswordService.Hash(model.Password),
            Role = Roles.Student,   // nobody can make themselves Admin from this form
            Profile = new StudentProfile
            {
                RollNo = roll,
                Branch = model.Branch,
                Cgpa = model.Cgpa,
                Backlogs = model.Backlogs
            }
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await SignInUser(user);
        TempData["Success"] = "Welcome to PlaceTrack! Your account is ready.";
        return RedirectToAction("Dashboard", "Student");
    }

    // ---------- LOGOUT ----------
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Denied() => View();

    // ---------- helpers ----------
    private async Task SignInUser(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }

    private IActionResult RedirectToDashboard(string role = null)
    {
        role ??= User.IsInRole(Roles.Admin) ? Roles.Admin : Roles.Student;
        return role == Roles.Admin
            ? RedirectToAction("Dashboard", "Admin")
            : RedirectToAction("Dashboard", "Student");
    }
}
