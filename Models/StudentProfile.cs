using System.ComponentModel.DataAnnotations;

namespace PlaceTrack.Models;

// Extra details only students have. One User (Student) has one StudentProfile.
public class StudentProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    [Required, StringLength(30)]
    public string RollNo { get; set; }

    [Required, StringLength(20)]
    public string Branch { get; set; }

    public decimal Cgpa { get; set; }
    public int Backlogs { get; set; }

    [StringLength(300)]
    public string Skills { get; set; }

    [StringLength(20)]
    public string Phone { get; set; }

    [StringLength(300)]
    public string ResumeUrl { get; set; }

    public List<Application> Applications { get; set; } = new();
}
