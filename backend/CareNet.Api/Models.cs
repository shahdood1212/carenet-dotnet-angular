using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace CareNet.Api;

public enum Role { Patient, Doctor, Admin }

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public Role Role { get; set; }
}

public class Doctor
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Specialty { get; set; } = "";
    public string? Bio { get; set; }
}

public class Appointment
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
    public int PatientId { get; set; }
    public User? Patient { get; set; }
    public DateTime StartsAt { get; set; }
}

public record RegisterDto([Required] string Name, [Required, EmailAddress] string Email, [Required, MinLength(6)] string Password);
public record LoginDto([Required] string Email, [Required] string Password);
public record AuthResponse(string Token, string Name, string Role);
public record DoctorDto([Required] string Name, [Required] string Specialty, string? Bio);
public record BookDto(int DoctorId, DateTime StartsAt);

public class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        m.Entity<User>().Property(u => u.Email).HasMaxLength(200);
        m.Entity<User>().HasIndex(u => u.Email).IsUnique();
        m.Entity<User>().Property(u => u.Role).HasConversion<string>();
        m.Entity<Appointment>().HasIndex(a => new { a.DoctorId, a.StartsAt }).IsUnique();
        m.Entity<Appointment>().HasOne(a => a.Patient).WithMany().HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<Doctor>().HasData(
            new Doctor { Id = 1, Name = "Dr. Ahmed Hassan", Specialty = "Cardiology", Bio = "15 years of experience in heart care." },
            new Doctor { Id = 2, Name = "Dr. Sara Mostafa", Specialty = "Dermatology", Bio = "Skin and hair specialist." },
            new Doctor { Id = 3, Name = "Dr. Omar Khaled", Specialty = "Pediatrics", Bio = "Caring for children of all ages." });
    }
}
