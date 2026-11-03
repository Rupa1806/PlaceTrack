using Microsoft.EntityFrameworkCore;
using PlaceTrack.Models;

namespace PlaceTrack.Data;

// The DbContext is our "door" to the database.
// Each DbSet = one table.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Drive> Drives => Set<Drive>();
    public DbSet<Application> Applications => Set<Application>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Two people cannot share an email or a roll number.
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        b.Entity<StudentProfile>().HasIndex(s => s.RollNo).IsUnique();

        // User 1 --- 1 StudentProfile
        b.Entity<StudentProfile>()
            .HasOne(s => s.User)
            .WithOne(u => u.Profile)
            .HasForeignKey<StudentProfile>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Company 1 --- many Drives
        b.Entity<Drive>()
            .HasOne(d => d.Company)
            .WithMany(c => c.Drives)
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Drive 1 --- many Applications, StudentProfile 1 --- many Applications
        b.Entity<Application>()
            .HasOne(a => a.Drive)
            .WithMany(d => d.Applications)
            .HasForeignKey(a => a.DriveId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Application>()
            .HasOne(a => a.StudentProfile)
            .WithMany(s => s.Applications)
            .HasForeignKey(a => a.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // A student can apply to the same drive only ONCE (the database enforces it).
        b.Entity<Application>()
            .HasIndex(a => new { a.DriveId, a.StudentProfileId })
            .IsUnique();

        b.Entity<Drive>().Property(d => d.PackageLpa).HasPrecision(5, 2);
        b.Entity<Drive>().Property(d => d.MinCgpa).HasPrecision(4, 2);
        b.Entity<StudentProfile>().Property(s => s.Cgpa).HasPrecision(4, 2);
    }
}
