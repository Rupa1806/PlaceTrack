namespace PlaceTrack.Models;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Student = "Student";
}

public static class Statuses
{
    public const string Applied = "Applied";
    public const string Shortlisted = "Shortlisted";
    public const string Selected = "Selected";
    public const string Rejected = "Rejected";

    public static readonly string[] All = { Applied, Shortlisted, Selected, Rejected };
}

public static class Branches
{
    public static readonly string[] All = { "CSE", "IT", "ECE", "EEE", "MECH", "CIVIL" };
}
