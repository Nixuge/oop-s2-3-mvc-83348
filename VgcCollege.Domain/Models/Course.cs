namespace VgcCollege.Domain.Models;

public class Course {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public List<CourseEnrolment> Enrolments { get; set; } = new();
    public List<Assignment> Assignments { get; set; } = new();
    public List<Exam> Exams { get; set; } = new();
    
    public string? FacultyProfileId { get; set; } // Simplified: each course has one main faculty?
    public FacultyProfile? FacultyProfile { get; set; }
}