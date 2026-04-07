using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class StudentsController : BaseController {
    public StudentsController(AppDbContext context, UserManager<IdentityUser> userManager) 
        : base(context, userManager) { }

    public async Task<IActionResult> Index() {
        return View(await _context.StudentProfiles.ToListAsync());
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentProfile profile, string password) {
        if (ModelState.IsValid) {
            var user = new IdentityUser { UserName = profile.Email, Email = profile.Email };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded) {
                await _userManager.AddToRoleAsync(user, "Student");
                profile.IdentityUserId = user.Id;
                _context.Add(profile);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
        }
        return View(profile);
    }

    public async Task<IActionResult> Edit(string id) {
        var student = await _context.StudentProfiles.FindAsync(id);
        if (student == null) return NotFound();
        return View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, StudentProfile profile) {
        if (id != profile.Id) return NotFound();
        if (ModelState.IsValid) {
            _context.Update(profile);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(profile);
    }

    public async Task<IActionResult> Details(string id) {
        var student = await _context.StudentProfiles
            .Include(s => s.Enrolments).ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) return NotFound();
        return View(student);
    }

    public async Task<IActionResult> Enrol(string id) {
        var student = await _context.StudentProfiles
            .Include(s => s.Enrolments)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) return NotFound();

        var enrolledCourseIds = student.Enrolments.Select(e => e.CourseId).ToList();
        var availableCourses = await _context.Courses
            .Where(c => !enrolledCourseIds.Contains(c.Id))
            .ToListAsync();

        ViewData["CourseId"] = new SelectList(availableCourses, "Id", "Name");
        return View(new CourseEnrolment { StudentProfileId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol([Bind("StudentProfileId,CourseId")] CourseEnrolment enrolment) {
        // Clear validation for navigation properties
        ModelState.Remove("Student");
        ModelState.Remove("Course");

        // Prevent duplicate enrolment server-side
        var alreadyEnrolled = await _context.CourseEnrolments
            .AnyAsync(e => e.StudentProfileId == enrolment.StudentProfileId && e.CourseId == enrolment.CourseId);

        if (alreadyEnrolled) {
            ModelState.AddModelError("", "Student is already enrolled in this course.");
        }

        if (ModelState.IsValid) {
            enrolment.EnrolDate = DateTime.Now;
            enrolment.Status = "Active";
            _context.Add(enrolment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = enrolment.StudentProfileId });
        }

        var student = await _context.StudentProfiles
            .Include(s => s.Enrolments)
            .FirstOrDefaultAsync(s => s.Id == enrolment.StudentProfileId);
        
        var enrolledCourseIds = student?.Enrolments.Select(e => e.CourseId).ToList() ?? new List<int>();
        var availableCourses = await _context.Courses
            .Where(c => !enrolledCourseIds.Contains(c.Id))
            .ToListAsync();

        ViewData["CourseId"] = new SelectList(availableCourses, "Id", "Name", enrolment.CourseId);
        return View(enrolment);
    }
}