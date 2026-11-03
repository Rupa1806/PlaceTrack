using System.ComponentModel.DataAnnotations;

namespace PlaceTrack.ViewModels;

public class LoginVM
{
    [Required(ErrorMessage = "Enter your email")]
    [EmailAddress(ErrorMessage = "Enter a valid email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Enter your password")]
    public string Password { get; set; }

    public string ReturnUrl { get; set; }
}

public class RegisterVM
{
    [Required(ErrorMessage = "Enter your full name"), StringLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Enter your email")]
    [EmailAddress(ErrorMessage = "Enter a valid email"), StringLength(150)]
    public string Email { get; set; }

    [Required(ErrorMessage = "Enter your roll number"), StringLength(30)]
    [Display(Name = "Roll number")]
    public string RollNo { get; set; }

    [Required(ErrorMessage = "Pick your branch")]
    public string Branch { get; set; }

    [Range(0, 10, ErrorMessage = "CGPA must be between 0 and 10")]
    public decimal Cgpa { get; set; }

    [Range(0, 20, ErrorMessage = "Backlogs must be between 0 and 20")]
    [Display(Name = "Current backlogs")]
    public int Backlogs { get; set; }

    [Required(ErrorMessage = "Choose a password"), MinLength(6, ErrorMessage = "Use at least 6 characters")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Type the password again")]
    [Compare("Password", ErrorMessage = "The two passwords do not match")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; }
}
