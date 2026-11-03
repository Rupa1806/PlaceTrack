# PlaceTrack: Interview Guide (read this tonight)

Speak slowly. Short, honest answers beat long confusing ones. If you do not know something, say: "I have not used that yet, but I understand the idea, and I would learn it."

---

## 1. Your 30-second introduction to the project

> "PlaceTrack is a campus placement management system I built with C#, ASP.NET Core MVC and MySQL. In my college, placement details move through Excel sheets and WhatsApp groups, so students miss drives and the placement cell cannot see the full picture. In PlaceTrack, the placement cell creates drives with eligibility rules, students see only the drives they qualify for, and both sides can track every application from Applied to Shortlisted to Selected."

## 2. The six questions they will ask

### Why did you make this project?
- I saw a real problem in my own college. Drive details are scattered and students apply to drives they are not eligible for, or miss the last date.
- A placement system has real parts: roles, rules, relationships and status flow. It is more than simple add, edit and delete.
- It let me practise a full application: login, database design, business rules and a dashboard.

### Why did you choose this tech stack?
- **C#**: I already know it well, and I built MVC projects before.
- **ASP.NET Core MVC**: it keeps the code clean (Model, View, Controller), and it has login, roles and CSRF protection built in.
- **Entity Framework Core**: I work with C# objects, not hand-written SQL, and it protects against SQL injection with parameterised queries.
- **MySQL**: a relational database fits this data (students, companies, drives, applications are linked). I also know MySQL from earlier projects.
- **Plain HTML and CSS**: I wanted full control of the design, and I wanted to show I understand CSS and not only a framework.
- **Chart.js**: a small library for the dashboard charts.

### What challenges did you face, and how did you solve them?
1. **Eligibility rules.** Each drive has its own CGPA, backlog and branch rules. I put one method, `Drive.WhyNotEligible(student)`, in one place. It returns null if the student can apply, or a plain sentence saying why not. The same method is used for the page, the Apply button and the server check, so the rules never disagree.
2. **Duplicate applications.** A double click or two tabs could create two applications. I added a **unique index** on (DriveId, StudentProfileId), so the database itself refuses a duplicate, and I catch that error and show a friendly message.
3. **Security.** Passwords are hashed with PBKDF2 and a random salt. Login uses cookies with roles, so students cannot open admin pages. Every POST form has an anti-forgery token. A student cannot make themselves an admin because the register form always creates the Student role.
4. **Trusting the browser.** The Apply button is hidden for ineligible students, but anyone can fake a request. So the server checks eligibility again before saving.
5. **MySQL provider version.** The MySQL provider for EF Core (Pomelo) did not yet support EF Core 10, so I used EF Core 9 packages, which run fine on .NET 10.
6. **First-run setup.** The app creates the database, tables and sample data on first start, so anyone can run it with one command.

### What is the future scope?
- Email or SMS alerts when a status changes.
- Resume upload and automatic resume check against the job role.
- Interview scheduling with a calendar, and reminders before the last date.
- Admin verification of CGPA (so students cannot enter wrong marks).
- Company login so recruiters can see their own applicants.
- Move to EF Core Migrations, add unit tests, and deploy to Azure.

### What new features would you add?
- Eligibility alerts: "A new drive opened that matches you."
- Skill matching: compare student skills with the job description.
- Placement analytics for the principal: year-wise and company-wise reports.
- Mock test and preparation resources linked to each company.

### Why did you choose this project?
- "Because placement is something every student in my college lives through, so I understood the real problem. It also covers the parts companies care about: database design, security, business rules and a clean user interface."

---

## 3. Explain MVC with one example (they love this)

"When a student clicks **Apply now**:
1. The browser sends a POST request to `/Student/Apply`.
2. The **Controller** (`StudentController.Apply`) receives it. It finds the logged-in student from the cookie and loads the drive.
3. It asks the **Model** (`Drive.WhyNotEligible`) if the student can apply. The Model also talks to the database through Entity Framework.
4. If yes, it saves a new `Application` row and redirects.
5. The **View** (`MyApplications.cshtml`) shows the new application with its tracker line."

Short version: **Model = data and rules. View = what the user sees. Controller = takes the request, uses the Model, picks the View.**

## 4. Database design

| Table | Key columns | Notes |
|---|---|---|
| Users | Id, FullName, Email (unique), PasswordHash, Role | Admin and Student both live here |
| StudentProfiles | Id, UserId, RollNo (unique), Branch, Cgpa, Backlogs, Skills | One-to-one with Users |
| Companies | Id, Name, Industry, Website | |
| Drives | Id, CompanyId, JobRole, PackageLpa, MinCgpa, MaxBacklogs, AllowedBranches, LastDate, IsOpen | One company has many drives |
| Applications | Id, DriveId, StudentProfileId, Status, AppliedOn | **Unique (DriveId, StudentProfileId)** |

Relationships: Company 1:N Drive. Drive 1:N Application. StudentProfile 1:N Application. User 1:1 StudentProfile. Applications is the "join" between students and drives (many-to-many).

Why a separate StudentProfiles table? Admins are users too, but they do not have CGPA or roll numbers. Keeping student data separate avoids empty columns.

## 5. Demo script (about 4 minutes)

1. Home page: "This is the tracker idea: every application moves along a line."
2. Log in as **student@placetrack.com**. Open **Drives**: "Three drives are locked and each says why."
3. Apply to one drive. Show **My applications**.
4. Profile: raise CGPA, show the locked drives open up.
5. Log in as **admin@placetrack.com**: show **Dashboard**, create a drive with rules, open its applicants, change a status, download CSV.
6. Go back to the student: the status has changed.

## 6. Technical questions and short answers

- **What is ASP.NET Core?** A cross-platform framework from Microsoft for building web apps and APIs.
- **What is middleware?** Steps a request passes through in order. In `Program.cs`: static files, routing, authentication, authorization.
- **Authentication vs authorization?** Authentication = who are you (login). Authorization = what are you allowed to do (roles, `[Authorize(Roles = "Admin")]`).
- **What is an ORM?** Object Relational Mapper. It maps C# classes to database tables. Entity Framework Core is the ORM here.
- **What is DbContext?** The class that represents a session with the database. Each `DbSet` is a table.
- **EnsureCreated vs Migrations?** EnsureCreated builds the tables once from the models. Migrations track every later change to the schema. I would use Migrations in production.
- **What is Pomelo?** The provider (adapter) that lets EF Core talk to MySQL.
- **How do you stop SQL injection?** EF Core sends parameterised queries, so user input is never joined into SQL text.
- **What is CSRF and how did you stop it?** Another site tricking your browser into sending a request. Every form has a hidden anti-forgery token, checked automatically by `AutoValidateAntiforgeryToken`.
- **How are passwords stored?** Never as text. PBKDF2 with a random salt and 100,000 rounds, compared in fixed time.
- **Why view models?** So a form only accepts the fields I choose, which prevents over-posting.
- **ViewBag vs TempData?** ViewBag lasts for one request (controller to view). TempData survives one redirect, which I use for success messages.
- **What does `Include` do?** It loads related data in the same query (for example the Company with each Drive), which avoids many small queries.
- **What is dependency injection?** The framework gives my controller its `AppDbContext` through the constructor, so I do not create it myself.
- **Async and await?** The thread is free to serve other requests while the database is working.
- **What is Razor?** HTML with C# inside it, so pages can show data.
- **Why LINQ in memory for the dashboard?** The data is small, and it keeps the code easy to read. For huge data I would group in the database.
- **How would you scale it?** Add paging, caching for dashboard numbers, indexes on common filters, and deploy behind a real web server.

## 7. Be honest about the limits (this builds trust)

- "Students type their own CGPA. In real life the college would verify it, and that is in my future scope."
- "I did not write automated tests yet. I tested the flows by hand."
- "Tables are created with EnsureCreated. For production I would switch to Migrations."
- "Emails and resume upload are not built yet."

## 8. Tips for tomorrow

- Run the app once tonight and click every page. Know where each file is.
- Be ready to open `Drive.cs` and read the `WhyNotEligible` method line by line. It is your best piece of code.
- Your resume should say C# and ASP.NET Core MVC for this project (see below).
- If asked about EduPath (Spring Boot): "I built it to learn Spring Boot. PlaceTrack uses C# because it is the stack I am strongest in." Be ready to explain Controller, Service, Repository.

## 9. Resume lines for this project

**PlaceTrack: Campus Placement Management System** (Individual)
Tech Stack: C#, ASP.NET Core MVC (.NET 10), Entity Framework Core, MySQL, HTML5, CSS3, JavaScript
- Built a role-based placement portal for students and the placement cell using MVC architecture, with secure cookie login and hashed passwords.
- Implemented rule-based eligibility (CGPA, backlogs, branch) so students see only drives they qualify for, with a clear reason when they do not.
- Designed a relational schema with unique constraints to prevent duplicate applications, plus an admin dashboard with placement statistics and CSV export.
