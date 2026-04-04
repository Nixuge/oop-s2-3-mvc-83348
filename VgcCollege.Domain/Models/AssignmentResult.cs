namespace VgcCollege.Domain.Models;

public class AssignmentResult {
    public int Id { get; set; }
    public int AssignmentId { get; set; }
    public Assignment? Assignment { get; set; }
    public string StudentProfileId { get; set; } = string.Empty;
    public StudentProfile? Student { get; set; }
    public double Score { get; set; }
    public string Feedback { get; set; } = string.Empty;
}