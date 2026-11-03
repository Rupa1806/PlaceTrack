using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlaceTrack.Data;
using PlaceTrack.Models;

namespace PlaceTrack.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    public HomeController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewBag.Students = await _db.StudentProfiles.CountAsync();
        ViewBag.Companies = await _db.Companies.CountAsync();
        ViewBag.OpenDrives = await _db.Drives.CountAsync(d => d.IsOpen);
        ViewBag.Offers = await _db.Applications.CountAsync(a => a.Status == Statuses.Selected);
        return View();
    }

    public IActionResult Error() => View();
}
