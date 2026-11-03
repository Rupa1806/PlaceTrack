using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using PlaceTrack.Models;

namespace PlaceTrack.ViewModels;

public class AdminDashboardVM
{
    public int TotalStudents { get; set; }
    public int PlacedStudents { get; set; }
    public int TotalCompanies { get; set; }
    public int OpenDrives { get; set; }
    public int TotalApplications { get; set; }

    public double PlacementPercent => TotalStudents == 0 ? 0 : Math.Round(PlacedStudents * 100.0 / TotalStudents, 1);

    // Chart data
    public List<string> StatusLabels { get; set; } = new();
    public List<int> StatusValues { get; set; } = new();
    public List<string> OfferLabels { get; set; } = new();
    public List<int> OfferValues { get; set; } = new();
    public List<string> BranchLabels { get; set; } = new();
    public List<double> BranchValues { get; set; } = new();

    public List<Application> Recent { get; set; } = new();

    // Charts read their data from HTML attributes as JSON text.
    public string Json(object value) => JsonSerializer.Serialize(value);
}

public class DriveFormVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Pick a company")]
    [Display(Name = "Company")]
    public int? CompanyId { get; set; }

    [Required(ErrorMessage = "Enter the job role"), StringLength(100)]
    [Display(Name = "Job role")]
    public string JobRole { get; set; }

    [Range(0.5, 100, ErrorMessage = "Enter a package between 0.5 and 100 LPA")]
    [Display(Name = "Package (LPA)")]
    public decimal PackageLpa { get; set; }

    [Required(ErrorMessage = "Enter the location"), StringLength(100)]
    public string Location { get; set; }

    [Range(0, 10, ErrorMessage = "CGPA must be between 0 and 10")]
    [Display(Name = "Minimum CGPA")]
    public decimal MinCgpa { get; set; }

    [Range(0, 20, ErrorMessage = "Enter a number from 0 to 20")]
    [Display(Name = "Maximum backlogs allowed")]
    public int MaxBacklogs { get; set; }

    [Required(ErrorMessage = "Pick the last date")]
    [Display(Name = "Last date to apply")]
    public DateTime? LastDate { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    // Ticked branch boxes. Nothing ticked (or all ticked) means "ALL".
    public List<string> SelectedBranches { get; set; } = new();
}
