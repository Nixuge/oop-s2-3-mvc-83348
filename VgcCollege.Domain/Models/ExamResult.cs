namespace VgcCollege.Domain.Models;

public class ExamResult {
    public int Id { get; set; }
    public int ExamId { get; set; }
    public Exam? Exam { get; set; }
    public string StudentProfileId { get; set; } = string.Empty;
    public StudentProfile? Student { get; set; }
    public double Score { get; set; }
    public string Grade { get; set; } = string.Empty;
}