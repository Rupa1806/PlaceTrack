using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlaceTrack.Data;
using PlaceTrack.Models;
using PlaceTrack.ViewModels;

namespace PlaceTrack.Controllers;

// Only logged-in Students can open this controller.
[Authorize(Roles = Roles.Student)]
public class StudentController : Controller
{
    private readonly AppDbContext _db;
    public StudentController(AppDbContext db) => _db = db;

    // The logged-in user's Id comes from the login cookie (not from the URL, so nobody can fake it).
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

    private Task<StudentProfile> GetProfileAsync() =>
        _db.StudentProfiles.Include(s => s.User).FirstAsync(s => s.UserId == CurrentUserId);

    // ================= DASHBOARD =================
    public async Task<IActionResult> Dashboard()
    {
        var profile = await GetProfileAsync();
        var cards = await BuildCardsAsync(profile);
        var myApps = await GetMyApplicationsAsync(profile.Id);

        var vm = new StudentDashboardVM
        {
            Profile = profile,
            EligibleCount = cards.Count(c => c.IsEligible && !c.Applied),
            AppliedCount = myApps.Count,
            ShortlistedCount = myApps.Count(a => a.Status == Statuses.Shortlisted),
            SelectedCount = myApps.Count(a => a.Status == Statuses.Selected),
            TopDrives = cards.Where(c => c.IsEligible && !c.Applied).Take(3).ToList(),
            Recent = myApps.Take(4).ToList()
        };
        return View(vm);
    }

    // ================= DRIVES (eligible + not eligible with reason) =================
    public async Task<IActionResult> Drives()
    {
        var profile = await GetProfileAsync();
        var cards = await BuildCardsAsync(profile);
        ViewBag.Profile = profile;
        return View(cards);
    }

    [HttpPost]
    public async Task<IActionResult> Apply(int driveId)
    {
        var profile = await GetProfileAsync();
        var drive = await _db.Drives.Include(d => d.Company).FirstOrDefaultAsync(d => d.Id == driveId);
        if (drive == null) return NotFound();

        // The server checks again. We never trust the button on the page.
        var reason = drive.WhyNotEligible(profile);
        if (reason != null)
        {
            TempData["Error"] = reason;
            return RedirectToAction(nameof(Drives));
        }

        if (await _db.Applications.AnyAsync(a => a.DriveId == driveId && a.StudentProfileId == profile.Id))
        {
            TempData["Error"] = "You have already applied to this drive.";
            return RedirectToAction(nameof(Drives));
        }

        try
        {
            _db.Applications.Add(new Application { DriveId = driveId, StudentProfileId = profile.Id });
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Double click or two tabs at once: the unique index in the database stops the duplicate.
            TempData["Error"] = "You have already applied to this drive.";
            return RedirectToAction(nameof(Drives));
        }

        TempData["Success"] = $"Applied to {drive.Company.Name} ({drive.JobRole}). Track it under My applications.";
        return RedirectToAction(nameof(MyApplications));
    }

    // ================= MY APPLICATIONS =================
    public async Task<IActionResult> MyApplications()
    {
        var profile = await GetProfileAsync();
        return View(await GetMyApplicationsAsync(profile.Id));
    }

    [HttpPost]
    public async Task<IActionResult> Withdraw(int applicationId)
    {
        var profile = await GetProfileAsync();
        var app = await _db.Applications.FirstOrDefaultAsync(a => a.Id == applicationId && a.StudentProfileId == profile.Id);
        if (app == null) return NotFound();

        if (app.Status != Statuses.Applied)
        {
            TempData["Error"] = "You can withdraw only before the placement cell reviews it.";
            return RedirectToAction(nameof(MyApplications));
        }

        _db.Applications.Remove(app);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Application withdrawn.";
        return RedirectToAction(nameof(MyApplications));
    }

    // ================= PROFILE =================
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var p = await GetProfileAsync();
        return View(new ProfileVM
        {
            FullName = p.User.FullName, RollNo = p.RollNo, Email = p.User.Email,
            Branch = p.Branch, Cgpa = p.Cgpa, Backlogs = p.Backlogs,
            Skills = p.Skills, Phone = p.Phone, ResumeUrl = p.ResumeUrl
        });
    }

    [HttpPost]
    public async Task<IActionResult> Profile(ProfileVM model)
    {
        var p = await GetProfileAsync();
        model.RollNo = p.RollNo;
        model.Email = p.User.Email;

        if (!Branches.All.Contains(model.Branch))
            ModelState.AddModelError(nameof(model.Branch), "Pick a branch from the list.");

        if (!string.IsNullOrWhiteSpace(model.ResumeUrl) &&
            !(model.ResumeUrl.StartsWith("http://") || model.ResumeUrl.StartsWith("https://")))
            ModelState.AddModelError(nameof(model.ResumeUrl), "The resume link must start with http:// or https://");

        if (!ModelState.IsValid) return View(model);

        p.User.FullName = model.FullName.Trim();
        p.Branch = model.Branch;
        p.Cgpa = model.Cgpa;
        p.Backlogs = model.Backlogs;
        p.Skills = model.Skills?.Trim();
        p.Phone = model.Phone?.Trim();
        p.ResumeUrl = model.ResumeUrl?.Trim();
        await _db.SaveChangesAsync();

        TempData["Success"] = "Profile saved. Your eligible drives are updated.";
        return RedirectToAction(nameof(Profile));
    }

    // ================= helpers =================
    private Task<List<Application>> GetMyApplicationsAsync(int profileId) =>
        _db.Applications
            .Include(a => a.Drive).ThenInclude(d => d.Company)
            .Where(a => a.StudentProfileId == profileId)
            .OrderByDescending(a => a.UpdatedOn)
            .ToListAsync();

    private async Task<List<DriveCard>> BuildCardsAsync(StudentProfile profile)
    {
        var today = DateTime.Today;

        // Open drives, soonest deadline first.
        var drives = await _db.Drives
            .Include(d => d.Company)
            .Where(d => d.IsOpen && d.LastDate >= today)
            .OrderBy(d => d.LastDate)
            .ToListAsync();

        var applied = await _db.Applications
            .Where(a => a.StudentProfileId == profile.Id)
            .ToListAsync();

        var cards = new List<DriveCard>();
        foreach (var d in drives)
        {
            var existing = applied.FirstOrDefault(a => a.DriveId == d.Id);
            cards.Add(new DriveCard
            {
                Drive = d,
                Reason = d.WhyNotEligible(profile),
                Applied = existing != null,
                AppliedStatus = existing?.Status
            });
        }

        // Eligible drives first, then the rest.
        return cards.OrderByDescending(c => c.IsEligible).ThenBy(c => c.Drive.LastDate).ToList();
    }
}
