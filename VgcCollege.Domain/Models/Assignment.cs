namespace VgcCollege.Domain.Models;

public class Assignment {
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public string Title { get; set; } = string.Empty;
    public double MaxScore { get; set; }
    public DateTime DueDate { get; set; }

    public List<AssignmentResult> Results { get; set; } = new();
}