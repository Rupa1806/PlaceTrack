using System.ComponentModel.DataAnnotations;

namespace PlaceTrack.Models;

public class Company
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; }

    [StringLength(100)]
    public string Industry { get; set; }

    [StringLength(200)]
    public string Website { get; set; }

    public List<Drive> Drives { get; set; } = new();
}
