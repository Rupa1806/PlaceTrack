using System.ComponentModel.DataAnnotations;
using PlaceTrack.Models;

namespace PlaceTrack.ViewModels;

// One drive as one student sees it: can I apply, or why not?
public class DriveCard
{
    public Drive Drive { get; set; }
    public string Reason { get; set; }          // null = eligible
    public bool Applied { get; set; }
    public string AppliedStatus { get; set; }
    public bool IsEligible => Reason == null;
}

public class StudentDashboardVM
{
    public StudentProfile Profile { get; set; }
    public int EligibleCount { get; set; }
    public int AppliedCount { get; set; }
    public int ShortlistedCount { get; set; }
    public int SelectedCount { get; set; }
    public List<DriveCard> TopDrives { get; set; } = new();
    public List<Application> Recent { get; set; } = new();
}

public class ProfileVM
{
    [Required(ErrorMessage = "Enter your full name"), StringLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; }

    public string RollNo { get; set; }
    public string Email { get; set; }

    [Required(ErrorMessage = "Pick your branch")]
    public string Branch { get; set; }

    [Range(0, 10, ErrorMessage = "CGPA must be between 0 and 10")]
    public decimal Cgpa { get; set; }

    [Range(0, 20, ErrorMessage = "Backlogs must be between 0 and 20")]
    [Display(Name = "Current backlogs")]
    public int Backlogs { get; set; }

    [StringLength(300)]
    [Display(Name = "Skills")]
    public string Skills { get; set; }

    [StringLength(20)]
    public string Phone { get; set; }

    [StringLength(300)]
    [Display(Name = "Resume link")]
    public string ResumeUrl { get; set; }
}
