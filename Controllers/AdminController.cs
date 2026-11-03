using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlaceTrack.Data;
using PlaceTrack.Models;
using PlaceTrack.ViewModels;

namespace PlaceTrack.Controllers;

// Only people with the Admin role can open anything in this controller.
[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

    // ================= DASHBOARD =================
    public async Task<IActionResult> Dashboard()
    {
        var students = await _db.StudentProfiles.Include(s => s.Applications).ToListAsync();
        var allStatuses = await _db.Applications.Select(a => a.Status).ToListAsync();
        var selected = await _db.Applications
            .Include(a => a.Drive).ThenInclude(d => d.Company)
            .Where(a => a.Status == Statuses.Selected)
            .ToListAsync();

        var vm = new AdminDashboardVM
        {
            TotalStudents = students.Count,
            PlacedStudents = students.Count(s => s.Applications.Any(a => a.Status == Statuses.Selected)),
            TotalCompanies = await _db.Companies.CountAsync(),
            OpenDrives = await _db.Drives.CountAsync(d => d.IsOpen),
            TotalApplications = allStatuses.Count
        };

        // Chart 1: applications by status
        foreach (var status in Statuses.All)
        {
            vm.StatusLabels.Add(status);
            vm.StatusValues.Add(allStatuses.Count(s => s == status));
        }

        // Chart 2: offers per company (top 8)
        foreach (var g in selected.GroupBy(a => a.Drive.Company.Name).OrderByDescending(g => g.Count()).Take(8))
        {
            vm.OfferLabels.Add(g.Key);
            vm.OfferValues.Add(g.Count());
        }

        // Chart 3: placement % by branch
        foreach (var g in students.GroupBy(s => s.Branch).OrderBy(g => g.Key))
        {
            int placed = g.Count(s => s.Applications.Any(a => a.Status == Statuses.Selected));
            vm.BranchLabels.Add(g.Key);
            vm.BranchValues.Add(Math.Round(placed * 100.0 / g.Count(), 1));
        }

        vm.Recent = await _db.Applications
            .Include(a => a.StudentProfile).ThenInclude(s => s.User)
            .Include(a => a.Drive).ThenInclude(d => d.Company)
            .OrderByDescending(a => a.UpdatedOn)
            .Take(6)
            .ToListAsync();

        return View(vm);
    }

    // ================= COMPANIES =================
    public async Task<IActionResult> Companies()
    {
        var list = await _db.Companies.Include(c => c.Drives).OrderBy(c => c.Name).ToListAsync();
        return View(list);
    }

    [HttpPost]
    public async Task<IActionResult> AddCompany(string name, string industry, string website)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Company name is required.";
            return RedirectToAction(nameof(Companies));
        }

        name = name.Trim();
        if (await _db.Companies.AnyAsync(c => c.Name == name))
        {
            TempData["Error"] = $"{name} is already in the list.";
            return RedirectToAction(nameof(Companies));
        }

        _db.Companies.Add(new Company { Name = name, Industry = industry?.Trim(), Website = website?.Trim() });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"{name} added.";
        return RedirectToAction(nameof(Companies));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var company = await _db.Companies.Include(c => c.Drives).FirstOrDefaultAsync(c => c.Id == id);
        if (company == null) return NotFound();

        if (company.Drives.Any())
        {
            TempData["Error"] = $"{company.Name} has drives. Delete those drives first.";
            return RedirectToAction(nameof(Companies));
        }

        _db.Companies.Remove(company);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"{company.Name} deleted.";
        return RedirectToAction(nameof(Companies));
    }

    // ================= DRIVES =================
    public async Task<IActionResult> Drives()
    {
        var list = await _db.Drives
            .Include(d => d.Company)
            .Include(d => d.Applications)
            .OrderByDescending(d => d.LastDate)
            .ToListAsync();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> DriveForm(int? id)
    {
        var vm = new DriveFormVM { LastDate = DateTime.Today.AddDays(14), MinCgpa = 6.0m, MaxBacklogs = 0, SelectedBranches = Branches.All.ToList() };

        if (id.HasValue)
        {
            var drive = await _db.Drives.FindAsync(id.Value);
            if (drive == null) return NotFound();

            vm = new DriveFormVM
            {
                Id = drive.Id,
                CompanyId = drive.CompanyId,
                JobRole = drive.JobRole,
                PackageLpa = drive.PackageLpa,
                Location = drive.Location,
                MinCgpa = drive.MinCgpa,
                MaxBacklogs = drive.MaxBacklogs,
                LastDate = drive.LastDate,
                Description = drive.Description,
                SelectedBranches = drive.AllowedBranches.Trim().ToUpper() == "ALL"
                    ? Branches.All.ToList()
                    : drive.AllowedBranches.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            };
        }

        await LoadCompaniesAsync(vm.CompanyId);
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> DriveForm(DriveFormVM model)
    {
        if (!ModelState.IsValid)
        {
            await LoadCompaniesAsync(model.CompanyId);
            return View(model);
        }

        // Nothing ticked, or everything ticked  =>  "ALL"
        var picked = (model.SelectedBranches ?? new List<string>()).Where(b => Branches.All.Contains(b)).Distinct().ToList();
        string allowed = (picked.Count == 0 || picked.Count == Branches.All.Length) ? "ALL" : string.Join(",", picked);

        Drive drive;
        if (model.Id == 0)
        {
            drive = new Drive();
            _db.Drives.Add(drive);
        }
        else
        {
            drive = await _db.Drives.FindAsync(model.Id);
            if (drive == null) return NotFound();
        }

        drive.CompanyId = model.CompanyId.Value;
        drive.JobRole = model.JobRole.Trim();
        drive.PackageLpa = model.PackageLpa;
        drive.Location = model.Location.Trim();
        drive.MinCgpa = model.MinCgpa;
        drive.MaxBacklogs = model.MaxBacklogs;
        drive.AllowedBranches = allowed;
        drive.LastDate = model.LastDate.Value.Date;
        drive.Description = model.Description?.Trim();

        await _db.SaveChangesAsync();
        TempData["Success"] = model.Id == 0 ? "Drive created. Eligible students can see it now." : "Drive updated.";
        return RedirectToAction(nameof(Drives));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleDrive(int id)
    {
        var drive = await _db.Drives.FindAsync(id);
        if (drive == null) return NotFound();

        drive.IsOpen = !drive.IsOpen;
        await _db.SaveChangesAsync();
        TempData["Success"] = drive.IsOpen ? "Drive reopened." : "Drive closed. Students can no longer apply.";
        return RedirectToAction(nameof(Drives));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteDrive(int id)
    {
        var drive = await _db.Drives.FindAsync(id);
        if (drive == null) return NotFound();

        _db.Drives.Remove(drive);   // its applications are removed too (cascade)
        await _db.SaveChangesAsync();
        TempData["Success"] = "Drive deleted.";
        return RedirectToAction(nameof(Drives));
    }

    // ================= APPLICANTS =================
    public async Task<IActionResult> Applicants(int id, string status = null)
    {
        var drive = await _db.Drives.Include(d => d.Company).FirstOrDefaultAsync(d => d.Id == id);
        if (drive == null) return NotFound();

        var apps = await _db.Applications
            .Include(a => a.StudentProfile).ThenInclude(s => s.User)
            .Where(a => a.DriveId == id)
            .ToListAsync();

        ViewBag.Drive = drive;
        ViewBag.Filter = status;
        ViewBag.Counts = Statuses.All.ToDictionary(s => s, s => apps.Count(a => a.Status == s));

        if (!string.IsNullOrEmpty(status) && Statuses.All.Contains(status))
            apps = apps.Where(a => a.Status == status).ToList();

        return View(apps.OrderByDescending(a => a.StudentProfile.Cgpa).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int applicationId, string status)
    {
        var app = await _db.Applications.FindAsync(applicationId);
        if (app == null) return NotFound();

        if (!Statuses.All.Contains(status))
        {
            TempData["Error"] = "That status is not valid.";
            return RedirectToAction(nameof(Applicants), new { id = app.DriveId });
        }

        app.Status = status;
        app.UpdatedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Status changed to {status}.";
        return RedirectToAction(nameof(Applicants), new { id = app.DriveId });
    }

    public async Task<IActionResult> ExportApplicants(int id)
    {
        var drive = await _db.Drives.Include(d => d.Company).FirstOrDefaultAsync(d => d.Id == id);
        if (drive == null) return NotFound();

        var apps = await _db.Applications
            .Include(a => a.StudentProfile).ThenInclude(s => s.User)
            .Where(a => a.DriveId == id)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Name,Roll No,Branch,CGPA,Backlogs,Email,Resume,Status");
        foreach (var a in apps.OrderByDescending(a => a.StudentProfile.Cgpa))
        {
            var s = a.StudentProfile;
            sb.AppendLine(string.Join(",", Csv(s.User.FullName), Csv(s.RollNo), Csv(s.Branch), s.Cgpa.ToString("0.00"), s.Backlogs, Csv(s.User.Email), Csv(s.ResumeUrl), Csv(a.Status)));
        }

        var fileName = $"applicants-{drive.Company.Name}-{drive.JobRole}.csv".Replace(" ", "-");
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName);
    }

    // ================= STUDENTS =================
    public async Task<IActionResult> Students(string q = null)
    {
        var query = _db.StudentProfiles
            .Include(s => s.User)
            .Include(s => s.Applications)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(s => s.User.FullName.Contains(q) || s.RollNo.Contains(q) || s.Branch.Contains(q));
        }

        ViewBag.Query = q;
        return View(await query.OrderBy(s => s.RollNo).ToListAsync());
    }

    // ================= helpers =================
    private async Task LoadCompaniesAsync(int? selectedId)
    {
        var companies = await _db.Companies.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Companies = new SelectList(companies, "Id", "Name", selectedId);
    }

    private static string Csv(string value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
}
