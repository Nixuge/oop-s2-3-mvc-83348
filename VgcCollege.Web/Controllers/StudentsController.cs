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

    public async Task<IActionResult> Details(string id) {
        var student = await _context.StudentProfiles
            .Include(s => s.Enrolments).ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) return NotFound();
        return View(student);
    }

    public async Task<IActionResult> Enrol(string id) {
        var student = await _context.StudentProfiles.FindAsync(id);
        if (student == null) return NotFound();
        ViewData["CourseId"] = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name");
        return View(new CourseEnrolment { StudentProfileId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(CourseEnrolment enrolment) {
        if (ModelState.IsValid) {
            enrolment.EnrolDate = DateTime.Now;
            enrolment.Status = "Active";
            _context.Add(enrolment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = enrolment.StudentProfileId });
        }
        ViewData["CourseId"] = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name", enrolment.CourseId);
        return View(enrolment);
    }
}