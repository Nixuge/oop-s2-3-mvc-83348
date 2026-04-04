using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class CoursesController : BaseController {
    public CoursesController(AppDbContext context, UserManager<IdentityUser> userManager) 
        : base(context, userManager) { }

    public async Task<IActionResult> Index() {
        return View(await _context.Courses.Include(c => c.Branch).Include(c => c.FacultyProfile).ToListAsync());
    }

    public async Task<IActionResult> Create() {
        ViewData["BranchId"] = new SelectList(await _context.Branches.ToListAsync(), "Id", "Name");
        ViewData["FacultyProfileId"] = new SelectList(await _context.FacultyProfiles.ToListAsync(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course course) {
        if (ModelState.IsValid) {
            _context.Add(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["BranchId"] = new SelectList(await _context.Branches.ToListAsync(), "Id", "Name", course.BranchId);
        ViewData["FacultyProfileId"] = new SelectList(await _context.FacultyProfiles.ToListAsync(), "Id", "Name", course.FacultyProfileId);
        return View(course);
    }

    public async Task<IActionResult> ManageResults(int id) {
        var course = await _context.Courses
            .Include(c => c.Exams)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return NotFound();
        return View(course);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleExamResults(int examId, bool released) {
        var exam = await _context.Exams.FindAsync(examId);
        if (exam != null) {
            exam.ResultsReleased = released;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(ManageResults), new { id = exam?.CourseId });
    }
}