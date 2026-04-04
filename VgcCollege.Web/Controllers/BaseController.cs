using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

public abstract class BaseController : Controller {
    protected readonly AppDbContext _context;
    protected readonly UserManager<IdentityUser> _userManager;

    protected BaseController(AppDbContext context, UserManager<IdentityUser> userManager) {
        _context = context;
        _userManager = userManager;
    }

    protected async Task<StudentProfile?> GetCurrentStudentProfileAsync() {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return null;
        return await _context.StudentProfiles.FirstOrDefaultAsync(s => s.IdentityUserId == user.Id);
    }

    protected async Task<FacultyProfile?> GetCurrentFacultyProfileAsync() {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return null;
        return await _context.FacultyProfiles.FirstOrDefaultAsync(f => f.IdentityUserId == user.Id);
    }
}