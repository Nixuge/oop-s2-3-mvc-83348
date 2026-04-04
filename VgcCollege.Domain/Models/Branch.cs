namespace VgcCollege.Domain.Models;

public class Branch {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public List<Course> Courses { get; set; } = new();
}