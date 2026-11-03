# PlaceTrack: Campus Placement Management System

Built with **C#, ASP.NET Core MVC (.NET 10), Entity Framework Core and MySQL**. Plain HTML and CSS (no Bootstrap). Charts use Chart.js.

## How to run (5 minutes)

1. **Install the .NET 10 SDK.** Check it with `dotnet --version` (should start with 10).
2. **Start MySQL** (the same one you used for your old servlet project is fine).
3. Open `appsettings.json` and put your MySQL password here:
   `Password=YOUR_MYSQL_PASSWORD;`  (and change `User=root` if your user is different)
4. Open a terminal **inside this folder** (where `PlaceTrack.csproj` is) and run:

   ```
   dotnet run
   ```
5. Open **http://localhost:5080** in your browser.

The database `placetrack`, all tables and the sample data are created **automatically** the first time.
To start fresh, drop the database in MySQL (`DROP DATABASE placetrack;`) and run again.

**Visual Studio:** .NET 10 needs a new Visual Studio (2026). If you have Visual Studio 2022 and it cannot open the project, just use the terminal command above. It always works.

## Demo accounts (sample data)

| Role | Email | Password |
|---|---|---|
| Placement officer (Admin) | admin@placetrack.com | Admin@123 |
| Student (CGPA 7.2, CSE) | student@placetrack.com | Student@123 |

The login page has buttons that fill these in for you. Remove them before putting the site on the internet.

## What is inside (simple map)

| Folder / file | What it does |
|---|---|
| `Program.cs` | Starts the app: MVC, database, login, then creates tables and sample data |
| `Models/` | The **M** of MVC. C# classes that become tables: User, StudentProfile, Company, Drive, Application. `Drive.cs` also holds the **eligibility check** |
| `Views/` | The **V**. The HTML pages (Razor). `Shared/_Layout.cshtml` is the common top bar |
| `Controllers/` | The **C**. `AccountController` (login, register), `AdminController`, `StudentController`, `HomeController` |
| `ViewModels/` | Small classes that carry form data and page data |
| `Data/AppDbContext.cs` | Tells Entity Framework about the tables and relationships |
| `Data/DbSeeder.cs` | Sample companies, drives, students and applications |
| `Services/PasswordService.cs` | Hashes passwords (PBKDF2 with salt) |
| `wwwroot/css/site.css` | All the design |
| `wwwroot/js/site.js` | Small interactions: search filter, count-up numbers, charts, confirm boxes |

## Try these in the demo

1. Log in as **student**. Open **Drives**. Three drives are locked, and each says exactly why (for example "Needs CGPA 8.0 or more. Yours is 7.2").
2. Click **Apply now** on an open drive. Go to **My applications** and see the tracker line.
3. Open **Profile**, change CGPA to 8.5, save, and open **Drives** again. The locked drives open up.
4. Log out, log in as **admin**. Open **Drives**, click the applicants link, change the student's status to **Selected**.
5. Open the admin **Dashboard**. The charts and placement percentage move.
6. Try **Download CSV** on an applicants page.

## Known limits (good to know before an interview)

- CGPA and backlogs are entered by the student. A real college would let the placement cell verify them.
- No email notifications and no resume file upload yet (a resume link field is used).
- Tables are created with `EnsureCreated()`. A production app would use EF Core Migrations.
- Not tested with automated unit tests.
