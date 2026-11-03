using System.ComponentModel.DataAnnotations;

namespace PlaceTrack.Models;

// One row per person who can log in (Admin or Student).
public class User
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string FullName { get; set; }

    [Required, StringLength(150)]
    public string Email { get; set; }

    // We NEVER store the real password. Only a salted hash (see PasswordService).
    [Required, StringLength(200)]
    public string PasswordHash { get; set; }

    [Required, StringLength(20)]
    public string Role { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public StudentProfile Profile { get; set; }
}
