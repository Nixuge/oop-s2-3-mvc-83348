namespace VgcCollege.Domain.Models;

public class Exam {
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public double MaxScore { get; set; }
    public bool ResultsReleased { get; set; }

    public List<ExamResult> Results { get; set; } = new();
}