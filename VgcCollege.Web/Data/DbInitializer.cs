using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;

namespace VgcCollege.Web.Data;

public static class DbInitializer {
    public static async Task SeedDataAsync(AppDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager) {
        // Roles
        string[] roles = { "Administrator", "Faculty", "Student" };
        foreach (var role in roles) {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Admin User
        if (await userManager.FindByEmailAsync("admin@nixuge.me") == null) {
            var user = new IdentityUser { UserName = "admin@nixuge.me", Email = "admin@nixuge.me" };
            await userManager.CreateAsync(user, "Securite2026!");
            await userManager.AddToRoleAsync(user, "Administrator");
        }

        // Faculty User
        FacultyProfile? facultyProfile = null;
        if (await userManager.FindByEmailAsync("faculty@nixuge.me") == null) {
            var user = new IdentityUser { UserName = "faculty@nixuge.me", Email = "faculty@nixuge.me" };
            await userManager.CreateAsync(user, "Securite2026!");
            await userManager.AddToRoleAsync(user, "Faculty");

            facultyProfile = new FacultyProfile {
                IdentityUserId = user.Id,
                Name = "Dr. John Smith",
                Email = user.Email,
                Phone = "123-456-7890"
            };
            db.FacultyProfiles.Add(facultyProfile);
            await db.SaveChangesAsync();
        } else {
            var user = await userManager.FindByEmailAsync("faculty@nixuge.me");
            facultyProfile = await db.FacultyProfiles.FirstOrDefaultAsync(f => f.IdentityUserId == user!.Id);
        }

        // Student Users
        var studentProfiles = new List<StudentProfile>();
        string[] studentEmails = { "student1@nixuge.me", "student2@nixuge.me" };
        foreach (var email in studentEmails) {
            if (await userManager.FindByEmailAsync(email) == null) {
                var user = new IdentityUser { UserName = email, Email = email };
                await userManager.CreateAsync(user, "Securite2026!");
                await userManager.AddToRoleAsync(user, "Student");

                var profile = new StudentProfile {
                    IdentityUserId = user.Id,
                    Name = email.Split('@')[0],
                    Email = user.Email,
                    Phone = "987-654-3210",
                    Address = "123 Student St",
                    StudentNumber = "S" + Guid.NewGuid().ToString().Substring(0, 8),
                    DateOfBirth = new DateTime(2000, 1, 1)
                };
                db.StudentProfiles.Add(profile);
                studentProfiles.Add(profile);
            } else {
                var user = await userManager.FindByEmailAsync(email);
                var profile = await db.StudentProfiles.FirstOrDefaultAsync(s => s.IdentityUserId == user!.Id);
                if (profile != null) studentProfiles.Add(profile);
            }
        }
        await db.SaveChangesAsync();

        // Branches
        if (!db.Branches.Any()) {
            db.Branches.AddRange(
                new Branch { Name = "Dublin", Address = "O'Connell Street, Dublin" },
                new Branch { Name = "Cork", Address = "St Patrick's Street, Cork" },
                new Branch { Name = "Galway", Address = "Shop Street, Galway" }
            );
            await db.SaveChangesAsync();
        }

        // Courses
        if (!db.Courses.Any()) {
            var dublinBranch = await db.Branches.FirstAsync(b => b.Name == "Dublin");
            var corkBranch = await db.Branches.FirstAsync(b => b.Name == "Cork");
            var galwayBranch = await db.Branches.FirstAsync(b => b.Name == "Galway");
            
            var courses = new List<Course> {
                // Dublin
                new Course { Name = "Computer Science", BranchId = dublinBranch.Id, StartDate = DateTime.Now.AddMonths(-1), EndDate = DateTime.Now.AddMonths(11), FacultyProfileId = facultyProfile?.Id },
                new Course { Name = "Data Analytics", BranchId = dublinBranch.Id, StartDate = DateTime.Now.AddMonths(-2), EndDate = DateTime.Now.AddMonths(10), FacultyProfileId = facultyProfile?.Id },
                new Course { Name = "Cybersecurity", BranchId = dublinBranch.Id, StartDate = DateTime.Now.AddMonths(-1), EndDate = DateTime.Now.AddMonths(11), FacultyProfileId = facultyProfile?.Id },
                new Course { Name = "Artificial Intelligence", BranchId = dublinBranch.Id, StartDate = DateTime.Now.AddMonths(1), EndDate = DateTime.Now.AddMonths(13), FacultyProfileId = facultyProfile?.Id },
                
                // Cork
                new Course { Name = "Business Management", BranchId = corkBranch.Id, StartDate = DateTime.Now.AddMonths(-3), EndDate = DateTime.Now.AddMonths(9), FacultyProfileId = facultyProfile?.Id },
                new Course { Name = "Digital Marketing", BranchId = corkBranch.Id, StartDate = DateTime.Now.AddMonths(-1), EndDate = DateTime.Now.AddMonths(11), FacultyProfileId = facultyProfile?.Id },
                new Course { Name = "Project Management", BranchId = corkBranch.Id, StartDate = DateTime.Now.AddMonths(2), EndDate = DateTime.Now.AddMonths(14), FacultyProfileId = facultyProfile?.Id },
                
                // Galway
                new Course { Name = "Creative Media", BranchId = galwayBranch.Id, StartDate = DateTime.Now.AddMonths(-2), EndDate = DateTime.Now.AddMonths(10), FacultyProfileId = facultyProfile?.Id },
                new Course { Name = "Psychology", BranchId = galwayBranch.Id, StartDate = DateTime.Now.AddMonths(-1), EndDate = DateTime.Now.AddMonths(11), FacultyProfileId = facultyProfile?.Id },
                new Course { Name = "Sociology", BranchId = galwayBranch.Id, StartDate = DateTime.Now.AddMonths(1), EndDate = DateTime.Now.AddMonths(13), FacultyProfileId = facultyProfile?.Id },
                new Course { Name = "Nursing", BranchId = galwayBranch.Id, StartDate = DateTime.Now.AddMonths(-4), EndDate = DateTime.Now.AddMonths(8), FacultyProfileId = facultyProfile?.Id }
            };
            db.Courses.AddRange(courses);
            await db.SaveChangesAsync();
        }

        // Enrolments
        if (!db.CourseEnrolments.Any()) {
            var courses = await db.Courses.Take(2).ToListAsync(); // Only enrol in the first 2 courses
            foreach (var student in studentProfiles) {
                foreach (var course in courses) {
                    db.CourseEnrolments.Add(new CourseEnrolment {
                        StudentProfileId = student.Id,
                        CourseId = course.Id,
                        EnrolDate = DateTime.Now.AddMonths(-1),
                        Status = "Active"
                    });
                }
            }
            await db.SaveChangesAsync();
        }

        // Attendance, Assignments, Exams
        if (!db.AttendanceRecords.Any()) {
            var enrolments = await db.CourseEnrolments.ToListAsync();
            foreach (var enrolment in enrolments) {
                for (int i = 1; i <= 4; i++) {
                    db.AttendanceRecords.Add(new AttendanceRecord {
                        CourseEnrolmentId = enrolment.Id,
                        WeekNumber = i,
                        Date = DateTime.Now.AddDays(-7 * (4 - i)),
                        Present = i % 4 != 0 // mostly present
                    });
                }
            }
            await db.SaveChangesAsync();
        }

        if (!db.Assignments.Any()) {
            var coursesWithEnrolments = await db.Courses
                .Include(c => c.Enrolments)
                .Where(c => c.Enrolments.Any())
                .ToListAsync();

            foreach (var course in coursesWithEnrolments) {
                var assignment = new Assignment {
                    CourseId = course.Id,
                    Title = "Intro Assignment",
                    MaxScore = 100,
                    DueDate = DateTime.Now.AddDays(-10)
                };
                db.Assignments.Add(assignment);
                await db.SaveChangesAsync();

                foreach (var enrolment in course.Enrolments) {
                    db.AssignmentResults.Add(new AssignmentResult {
                        AssignmentId = assignment.Id,
                        StudentProfileId = enrolment.StudentProfileId,
                        Score = 85,
                        Feedback = "Well done!"
                    });
                }
            }
            await db.SaveChangesAsync();
        }

        if (!db.Exams.Any()) {
            var coursesWithEnrolments = await db.Courses
                .Include(c => c.Enrolments)
                .Where(c => c.Enrolments.Any())
                .ToListAsync();

            foreach (var course in coursesWithEnrolments) {
                var exam = new Exam {
                    CourseId = course.Id,
                    Title = "Midterm Exam",
                    Date = DateTime.Now.AddDays(-5),
                    MaxScore = 100,
                    ResultsReleased = false // Provisional
                };
                db.Exams.Add(exam);
                await db.SaveChangesAsync();

                foreach (var enrolment in course.Enrolments) {
                    db.ExamResults.Add(new ExamResult {
                        ExamId = exam.Id,
                        StudentProfileId = enrolment.StudentProfileId,
                        Score = 78,
                        Grade = "B"
                    });
                }
            }
            await db.SaveChangesAsync();
        }
    }
}