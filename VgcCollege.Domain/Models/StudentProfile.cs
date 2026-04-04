namespace VgcCollege.Domain.Models;

public class StudentProfile {
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string IdentityUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }

    public List<CourseEnrolment> Enrolments { get; set; } = new();
    public List<AssignmentResult> AssignmentResults { get; set; } = new();
    public List<ExamResult> ExamResults { get; set; } = new();
}