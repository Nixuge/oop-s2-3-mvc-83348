namespace VgcCollege.Domain.Models;

public class AttendanceRecord {
    public int Id { get; set; }
    public int CourseEnrolmentId { get; set; }
    public CourseEnrolment? CourseEnrolment { get; set; }
    public DateTime Date { get; set; }
    public int WeekNumber { get; set; }
    public bool Present { get; set; }
}