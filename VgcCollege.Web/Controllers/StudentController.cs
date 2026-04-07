using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : BaseController {
    public StudentController(AppDbContext context, UserManager<IdentityUser> userManager) 
        : base(context, userManager) { }

    public async Task<IActionResult> MyProfile() {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var profile = await _context.StudentProfiles
            .Include(s => s.Enrolments).ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.IdentityUserId == user.Id);

        if (profile == null) return NotFound("Student profile not found.");
        return View(profile);
    }

    public async Task<IActionResult> MyGrades() {
        var profile = await GetCurrentStudentProfileAsync();
        if (profile == null) return NotFound("Student profile not found.");

        var enrolments = await _context.CourseEnrolments
            .Where(e => e.StudentProfileId == profile.Id)
            .Include(e => e.Course).ThenInclude(c => c!.Assignments)
            .Include(e => e.Course).ThenInclude(c => c!.Exams)
            .ToListAsync();

        var assignmentResults = await _context.AssignmentResults
            .Where(r => r.StudentProfileId == profile.Id)
            .Include(r => r.Assignment)
            .ToListAsync();

        var examResults = await _context.ExamResults
            .Where(r => r.StudentProfileId == profile.Id)
            .Include(r => r.Exam)
            .Where(r => r.Exam!.ResultsReleased) // Constraint: Only released
            .ToListAsync();

        ViewBag.Enrolments = enrolments;
        ViewBag.AssignmentResults = assignmentResults;
        ViewBag.ExamResults = examResults;

        return View();
    }
}