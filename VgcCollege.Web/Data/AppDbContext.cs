using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;

namespace VgcCollege.Web.Data;

public class AppDbContext : IdentityDbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<FacultyProfile> FacultyProfiles => Set<FacultyProfile>();
    public DbSet<CourseEnrolment> CourseEnrolments => Set<CourseEnrolment>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<AssignmentResult> AssignmentResults => Set<AssignmentResult>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamResult> ExamResults => Set<ExamResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        
        // Relationships
        modelBuilder.Entity<Course>()
            .HasOne(c => c.Branch)
            .WithMany(b => b.Courses)
            .HasForeignKey(c => c.BranchId);

        modelBuilder.Entity<CourseEnrolment>()
            .HasOne(ce => ce.Student)
            .WithMany(s => s.Enrolments)
            .HasForeignKey(ce => ce.StudentProfileId);

        modelBuilder.Entity<CourseEnrolment>()
            .HasOne(ce => ce.Course)
            .WithMany(c => c.Enrolments)
            .HasForeignKey(ce => ce.CourseId);

        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(ar => ar.CourseEnrolment)
            .WithMany(ce => ce.AttendanceRecords)
            .HasForeignKey(ar => ar.CourseEnrolmentId);

        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Course)
            .WithMany(c => c.Assignments)
            .HasForeignKey(a => a.CourseId);

        modelBuilder.Entity<AssignmentResult>()
            .HasOne(ar => ar.Assignment)
            .WithMany(a => a.Results)
            .HasForeignKey(ar => ar.AssignmentId);

        modelBuilder.Entity<AssignmentResult>()
            .HasOne(ar => ar.Student)
            .WithMany(s => s.AssignmentResults)
            .HasForeignKey(ar => ar.StudentProfileId);

        modelBuilder.Entity<Exam>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Exams)
            .HasForeignKey(e => e.CourseId);

        modelBuilder.Entity<ExamResult>()
            .HasOne(er => er.Exam)
            .WithMany(e => e.Results)
            .HasForeignKey(er => er.ExamId);

        modelBuilder.Entity<ExamResult>()
            .HasOne(er => er.Student)
            .WithMany(s => s.ExamResults)
            .HasForeignKey(er => er.StudentProfileId);

        modelBuilder.Entity<Course>()
            .HasOne(c => c.FacultyProfile)
            .WithMany(f => f.AssignedCourses)
            .HasForeignKey(c => c.FacultyProfileId);
    }
}