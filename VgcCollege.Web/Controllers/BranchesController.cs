using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class BranchesController : BaseController {
    public BranchesController(AppDbContext context, UserManager<IdentityUser> userManager) 
        : base(context, userManager) { }

    public async Task<IActionResult> Index() {
        return View(await _context.Branches.ToListAsync());
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Branch branch) {
        if (ModelState.IsValid) {
            _context.Add(branch);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(branch);
    }
}