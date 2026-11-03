using System.ComponentModel.DataAnnotations;

namespace PlaceTrack.Models;

// A "drive" = one job opening from one company, with rules about who can apply.
public class Drive
{
    public int Id { get; set; }

    public int CompanyId { get; set; }
    public Company Company { get; set; }

    [Required, StringLength(100)]
    public string JobRole { get; set; }

    public decimal PackageLpa { get; set; }

    [Required, StringLength(100)]
    public string Location { get; set; }

    // ----- Eligibility rules -----
    public decimal MinCgpa { get; set; }
    public int MaxBacklogs { get; set; }

    // "ALL" or a comma list like "CSE,IT,ECE"
    [Required, StringLength(100)]
    public string AllowedBranches { get; set; } = "ALL";

    public DateTime LastDate { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    public bool IsOpen { get; set; } = true;

    public List<Application> Applications { get; set; } = new();

    // ------------------------------------------------------------------
    // THE SMART PART: eligibility check.
    // Returns null when the student CAN apply.
    // Otherwise returns a plain sentence telling the student WHY not.
    // ------------------------------------------------------------------
    public string WhyNotEligible(StudentProfile student)
    {
        if (!IsOpen) return "This drive is closed.";
        if (LastDate.Date < DateTime.Today) return "The last date to apply has passed.";
        if (student.Cgpa < MinCgpa) return $"Needs CGPA {MinCgpa:0.0#} or more. Yours is {student.Cgpa:0.0#}.";
        if (student.Backlogs > MaxBacklogs) return $"Allows at most {MaxBacklogs} backlog(s). You have {student.Backlogs}.";
        if (!BranchAllowed(student.Branch)) return $"Open only for {AllowedBranches}. Your branch is {student.Branch}.";
        return null;
    }

    public bool BranchAllowed(string branch)
    {
        if (string.IsNullOrWhiteSpace(AllowedBranches) || AllowedBranches.Trim().ToUpper() == "ALL")
            return true;

        return AllowedBranches
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(b => string.Equals(b, branch, StringComparison.OrdinalIgnoreCase));
    }
}
