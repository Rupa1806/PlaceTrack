using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlaceTrack.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC. The filter below protects EVERY form POST from CSRF attacks.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// 2. Database: Entity Framework Core + MySQL (Pomelo provider).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// We give the MySQL version ourselves. (Auto-detect would try to connect to a database
// that does not exist yet on the very first run.) Works with MySQL 8.x and newer.
var mysqlVersion = new MySqlServerVersion(new Version(8, 0, 36));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, mysqlVersion));

// 3. Login: cookie authentication. Roles (Admin / Student) live inside the cookie.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Denied";
        options.Cookie.Name = "PlaceTrack.Auth";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(4);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// 4. On start: create the database and tables if they do not exist, then add sample data.
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    DbSeeder.Seed(db);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("\nPlaceTrack could not start because the database step failed.");
    Console.WriteLine("1) Is MySQL running?   2) Are the user and password in appsettings.json correct?");
    Console.WriteLine("Details: " + ex.GetBaseException().Message + "\n");
    Console.ResetColor();
    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();   // "Who are you?"
app.UseAuthorization();    // "Are you allowed here?"

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
