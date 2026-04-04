namespace VgcCollege.Domain.Models;

public class CourseEnrolment {
    public int Id { get; set; }
    public string StudentProfileId { get; set; } = string.Empty;
    public StudentProfile? Student { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public DateTime EnrolDate { get; set; }
    public string Status { get; set; } = "Active"; // Active, Withdrawn, etc.

    public List<AttendanceRecord> AttendanceRecords { get; set; } = new();
}