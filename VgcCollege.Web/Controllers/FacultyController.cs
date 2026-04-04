using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Faculty")]
public class FacultyController : BaseController {
    public FacultyController(AppDbContext context, UserManager<IdentityUser> userManager) 
        : base(context, userManager) { }

    public async Task<IActionResult> MyCourses() {
        var profile = await GetCurrentFacultyProfileAsync();
        if (profile == null) return NotFound("Faculty profile not found.");

        var courses = await _context.Courses
            .Where(c => c.FacultyProfileId == profile.Id)
            .Include(c => c.Branch)
            .ToListAsync();

        return View(courses);
    }

    public async Task<IActionResult> CourseDetails(int id) {
        var profile = await GetCurrentFacultyProfileAsync();
        var course = await _context.Courses
            .Include(c => c.Enrolments).ThenInclude(e => e.Student)
            .Include(c => c.Assignments)
            .Include(c => c.Exams)
            .FirstOrDefaultAsync(c => c.Id == id && c.FacultyProfileId == profile!.Id);

        if (course == null) return Unauthorized();

        return View(course);
    }

    public async Task<IActionResult> Gradebook(int courseId) {
        var profile = await GetCurrentFacultyProfileAsync();
        var course = await _context.Courses
            .Include(c => c.Assignments).ThenInclude(a => a.Results)
            .Include(c => c.Enrolments).ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(c => c.Id == courseId && c.FacultyProfileId == profile!.Id);

        if (course == null) return Unauthorized();

        return View(course);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAssignmentResult(int assignmentId, string studentProfileId, double score, string feedback) {
        var profile = await GetCurrentFacultyProfileAsync();
        var assignment = await _context.Assignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.Course!.FacultyProfileId == profile!.Id);

        if (assignment == null) return Unauthorized();

        var result = await _context.AssignmentResults
            .FirstOrDefaultAsync(r => r.AssignmentId == assignmentId && r.StudentProfileId == studentProfileId);

        if (result == null) {
            result = new AssignmentResult {
                AssignmentId = assignmentId,
                StudentProfileId = studentProfileId,
                Score = score,
                Feedback = feedback
            };
            _context.AssignmentResults.Add(result);
        } else {
            result.Score = score;
            result.Feedback = feedback;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Gradebook), new { courseId = assignment.CourseId });
    }

    public async Task<IActionResult> Attendance(int courseId, int? weekNumber) {
        var profile = await GetCurrentFacultyProfileAsync();
        var course = await _context.Courses
            .Include(c => c.Enrolments).ThenInclude(e => e.Student)
            .Include(c => c.Enrolments).ThenInclude(e => e.AttendanceRecords)
            .FirstOrDefaultAsync(c => c.Id == courseId && c.FacultyProfileId == profile!.Id);

        if (course == null) return Unauthorized();

        ViewBag.WeekNumber = weekNumber ?? 1;
        return View(course);
    }

    [HttpPost]
    public async Task<IActionResult> MarkAttendance(int enrolmentId, int weekNumber, bool present) {
        var enrolment = await _context.CourseEnrolments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == enrolmentId);
        
        var profile = await GetCurrentFacultyProfileAsync();
        if (enrolment == null || enrolment.Course?.FacultyProfileId != profile!.Id) return Unauthorized();

        var record = await _context.AttendanceRecords
            .FirstOrDefaultAsync(r => r.CourseEnrolmentId == enrolmentId && r.WeekNumber == weekNumber);

        if (record == null) {
            record = new AttendanceRecord {
                CourseEnrolmentId = enrolmentId,
                WeekNumber = weekNumber,
                Date = DateTime.Now,
                Present = present
            };
            _context.AttendanceRecords.Add(record);
        } else {
            record.Present = present;
            record.Date = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Attendance), new { courseId = enrolment.CourseId, weekNumber });
    }
}