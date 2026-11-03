using PlaceTrack.Models;
using PlaceTrack.Services;

namespace PlaceTrack.Data;

// Adds sample data the FIRST time the app runs, so every screen has something to show.
public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var today = DateTime.Today;

        // ---- Admin ----
        db.Users.Add(new User
        {
            FullName = "Placement Officer",
            Email = "admin@placetrack.com",
            PasswordHash = PasswordService.Hash("Admin@123"),
            Role = Roles.Admin
        });

        // ---- Companies (sample data) ----
        var companies = new List<Company>
        {
            new() { Name = "TCS",         Industry = "IT Services",   Website = "https://www.tcs.com" },
            new() { Name = "Infosys",     Industry = "IT Services",   Website = "https://www.infosys.com" },
            new() { Name = "Wipro",       Industry = "IT Services",   Website = "https://www.wipro.com" },
            new() { Name = "Accenture",   Industry = "Consulting",    Website = "https://www.accenture.com" },
            new() { Name = "Cognizant",   Industry = "IT Services",   Website = "https://www.cognizant.com" },
            new() { Name = "Capgemini",   Industry = "Consulting",    Website = "https://www.capgemini.com" },
            new() { Name = "JMAN Group",  Industry = "Data Analytics", Website = "https://www.jmangroup.com" },
            new() { Name = "Zoho",        Industry = "Product",       Website = "https://www.zoho.com" },
        };
        db.Companies.AddRange(companies);

        Company C(string name) => companies.First(c => c.Name == name);

        // ---- Drives with different rules ----
        var drives = new List<Drive>
        {
            new() { Company = C("TCS"), JobRole = "Assistant System Engineer", PackageLpa = 3.6m, Location = "Hyderabad",
                    MinCgpa = 6.0m, MaxBacklogs = 1, AllowedBranches = "ALL", LastDate = today.AddDays(12),
                    Description = "Entry level role. Training for 3 months, then project work." },
            new() { Company = C("Infosys"), JobRole = "Systems Engineer", PackageLpa = 3.6m, Location = "Pune",
                    MinCgpa = 6.5m, MaxBacklogs = 0, AllowedBranches = "ALL", LastDate = today.AddDays(9),
                    Description = "Online test, technical interview and HR round." },
            new() { Company = C("Wipro"), JobRole = "Project Engineer", PackageLpa = 3.5m, Location = "Bengaluru",
                    MinCgpa = 6.0m, MaxBacklogs = 1, AllowedBranches = "CSE,IT,ECE,EEE", LastDate = today.AddDays(15),
                    Description = "Aptitude, coding and communication rounds." },
            new() { Company = C("Accenture"), JobRole = "Associate Software Engineer", PackageLpa = 4.5m, Location = "Chennai",
                    MinCgpa = 6.5m, MaxBacklogs = 0, AllowedBranches = "CSE,IT,ECE", LastDate = today.AddDays(20),
                    Description = "Cognitive and technical assessment, then interview." },
            new() { Company = C("Cognizant"), JobRole = "Programmer Analyst Trainee", PackageLpa = 4.0m, Location = "Chennai",
                    MinCgpa = 7.0m, MaxBacklogs = 0, AllowedBranches = "CSE,IT,ECE,EEE", LastDate = today.AddDays(7),
                    Description = "Strong coding basics needed. Java, Python or C# is fine." },
            new() { Company = C("Capgemini"), JobRole = "Analyst", PackageLpa = 4.25m, Location = "Mumbai",
                    MinCgpa = 7.0m, MaxBacklogs = 0, AllowedBranches = "ALL", LastDate = today.AddDays(18),
                    Description = "Game based aptitude test followed by interview." },
            new() { Company = C("JMAN Group"), JobRole = "Data Analyst Trainee", PackageLpa = 7.0m, Location = "Chennai",
                    MinCgpa = 8.0m, MaxBacklogs = 0, AllowedBranches = "CSE,IT", LastDate = today.AddDays(10),
                    Description = "SQL, problem solving and a small case study. Good for data minded students." },
            new() { Company = C("Zoho"), JobRole = "Member Technical Staff", PackageLpa = 7.5m, Location = "Chennai",
                    MinCgpa = 7.5m, MaxBacklogs = 0, AllowedBranches = "CSE,IT,ECE", LastDate = today.AddDays(25),
                    Description = "Programming heavy rounds. Logic and clean code matter most." },
            new() { Company = C("TCS"), JobRole = "Digital Developer", PackageLpa = 7.0m, Location = "Hyderabad",
                    MinCgpa = 7.5m, MaxBacklogs = 0, AllowedBranches = "CSE,IT", LastDate = today.AddDays(14),
                    Description = "Advanced coding test. Higher package track." },
        };
        db.Drives.AddRange(drives);

        // ---- Students (all use the password Student@123) ----
        var studentHash = PasswordService.Hash("Student@123");
        var students = new List<StudentProfile>();

        void AddStudent(string name, string email, string roll, string branch, decimal cgpa, int backlogs, string skills)
        {
            var user = new User { FullName = name, Email = email, PasswordHash = studentHash, Role = Roles.Student };
            var profile = new StudentProfile
            {
                User = user, RollNo = roll, Branch = branch, Cgpa = cgpa, Backlogs = backlogs, Skills = skills
            };
            db.Users.Add(user);
            db.StudentProfiles.Add(profile);
            students.Add(profile);
        }

        // The demo student comes first. Easy to remember for the interview demo.
        AddStudent("Demo Student",    "student@placetrack.com",    "23CS001", "CSE",  7.20m, 0, "C#, Java, SQL");
        AddStudent("Anjali Reddy",    "anjali@college.edu",        "23CS002", "CSE",  9.10m, 0, "Java, Spring, MySQL");
        AddStudent("Karthik Varma",   "karthik@college.edu",       "23CS003", "CSE",  7.60m, 0, "Python, SQL");
        AddStudent("Sneha Patel",     "sneha@college.edu",         "23IT004", "IT",   8.20m, 0, "C#, .NET, SQL");
        AddStudent("Rahul Naidu",     "rahul@college.edu",         "23IT005", "IT",   6.80m, 1, "Java, HTML, CSS");
        AddStudent("Divya Lakshmi",   "divya@college.edu",         "23EC006", "ECE",  7.90m, 0, "C, Embedded, Python");
        AddStudent("Manoj Kumar",     "manoj@college.edu",         "23EC007", "ECE",  6.20m, 2, "C, Verilog");
        AddStudent("Priya Sharma",    "priya@college.edu",         "23EE008", "EEE",  7.10m, 0, "MATLAB, Python");
        AddStudent("Suresh Babu",     "suresh@college.edu",        "23ME009", "MECH", 6.60m, 1, "AutoCAD, SolidWorks");
        AddStudent("Harini Rao",      "harini@college.edu",        "23CS010", "CSE",  8.80m, 0, "Java, DSA, SQL");
        AddStudent("Vamsi Krishna",   "vamsi@college.edu",         "23CS011", "CSE",  7.30m, 0, "JavaScript, Node.js");
        AddStudent("Meghana Iyer",    "meghana@college.edu",       "23IT012", "IT",   8.00m, 0, "Python, Pandas, SQL");

        // ---- Applications: only where the student is really eligible ----
        var rnd = new Random(42);
        for (int i = 0; i < students.Count; i++)
        {
            var student = students[i];
            var eligible = drives.Where(d => d.WhyNotEligible(student) == null).ToList();

            // Demo student gets just two, so the demo can show "Apply" live.
            if (i == 0)
            {
                if (eligible.Count > 0) db.Applications.Add(new Application { Drive = eligible[0], StudentProfile = student, Status = Statuses.Shortlisted });
                if (eligible.Count > 1) db.Applications.Add(new Application { Drive = eligible[1], StudentProfile = student, Status = Statuses.Applied });
                continue;
            }

            foreach (var drive in eligible.OrderBy(_ => rnd.Next()).Take(3))
            {
                int roll = rnd.Next(100);
                string status = roll < 30 ? Statuses.Selected
                              : roll < 55 ? Statuses.Shortlisted
                              : roll < 75 ? Statuses.Rejected
                              : Statuses.Applied;

                db.Applications.Add(new Application
                {
                    Drive = drive,
                    StudentProfile = student,
                    Status = status,
                    AppliedOn = DateTime.UtcNow.AddDays(-rnd.Next(1, 12))
                });
            }
        }

        db.SaveChanges();
    }
}
