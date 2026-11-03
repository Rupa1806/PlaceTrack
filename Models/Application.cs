namespace PlaceTrack.Models;

// One row = one student applied to one drive. Status moves forward over time.
public class Application
{
    public int Id { get; set; }

    public int DriveId { get; set; }
    public Drive Drive { get; set; }

    public int StudentProfileId { get; set; }
    public StudentProfile StudentProfile { get; set; }

    public string Status { get; set; } = Statuses.Applied;

    public DateTime AppliedOn { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
}
