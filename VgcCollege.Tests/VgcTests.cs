using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using VgcCollege.Domain.Models;
using VgcCollege.Web.Controllers;
using VgcCollege.Web.Data;
using Xunit;

namespace VgcCollege.Tests;

public class VgcTests {
    private AppDbContext GetDbContext() {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private Mock<UserManager<IdentityUser>> GetUserManagerMock() {
        var store = new Mock<IUserStore<IdentityUser>>();
        return new Mock<UserManager<IdentityUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task Admin_CanCreateBranch() {
        // Arrange
        var context = GetDbContext();
        var userManager = GetUserManagerMock();
        var controller = new BranchesController(context, userManager.Object);

        // Act
        var branch = new Branch { Name = "Dublin", Address = "O'Connell St" };
        await controller.Create(branch);

        // Assert
        Assert.Equal(1, await context.Branches.CountAsync());
    }

    [Fact]
    public async Task Admin_CanEnrolStudent() {
        // Arrange
        var context = GetDbContext();
        var userManager = GetUserManagerMock();
        var controller = new StudentsController(context, userManager.Object);
        var student = new StudentProfile { Name = "John Doe" };
        context.StudentProfiles.Add(student);
        var course = new Course { Name = "CS" };
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        // Act
        var enrolment = new CourseEnrolment { StudentProfileId = student.Id, CourseId = course.Id };
        await controller.Enrol(enrolment);

        // Assert
        Assert.Equal(1, await context.CourseEnrolments.CountAsync());
    }

    [Fact]
    public async Task Student_CannotSeeProvisionalResults() {
        // Arrange
        var context = GetDbContext();
        var userManager = GetUserManagerMock();
        var studentId = "student-1";
        var userId = "user-1";
        
        var student = new StudentProfile { Id = studentId, IdentityUserId = userId };
        context.StudentProfiles.Add(student);
        
        var exam = new Exam { Title = "Midterm", ResultsReleased = false };
        context.Exams.Add(exam);
        
        var result = new ExamResult { Exam = exam, StudentProfileId = studentId, Score = 80 };
        context.ExamResults.Add(result);
        await context.SaveChangesAsync();

        var user = new IdentityUser { Id = userId, UserName = "student@nixuge.me" };
        userManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        var controller = new StudentController(context, userManager.Object);
        controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { 
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }, "mock")) }
        };

        // Act
        await controller.MyGrades();
        var examResults = controller.ViewBag.ExamResults as IEnumerable<ExamResult>;

        // Assert
        Assert.Empty(examResults!);
    }

    [Fact]
    public async Task Student_CanSeeReleasedResults() {
        // Arrange
        var context = GetDbContext();
        var userManager = GetUserManagerMock();
        var studentId = "student-1";
        var userId = "user-1";
        
        var student = new StudentProfile { Id = studentId, IdentityUserId = userId };
        context.StudentProfiles.Add(student);
        
        var exam = new Exam { Title = "Final", ResultsReleased = true };
        context.Exams.Add(exam);
        
        var result = new ExamResult { Exam = exam, StudentProfileId = studentId, Score = 90 };
        context.ExamResults.Add(result);
        await context.SaveChangesAsync();

        var user = new IdentityUser { Id = userId, UserName = "student@nixuge.me" };
        userManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        var controller = new StudentController(context, userManager.Object);
        controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { 
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }, "mock")) }
        };

        // Act
        await controller.MyGrades();
        var examResults = controller.ViewBag.ExamResults as IEnumerable<ExamResult>;

        // Assert
        Assert.Single(examResults!);
    }

    [Fact]
    public async Task Faculty_CanOnlySeeTheirAssignedCourses() {
        // Arrange
        var context = GetDbContext();
        var userManager = GetUserManagerMock();
        var facultyId = "faculty-100";
        var userId = "user-f100";
        
        var faculty = new FacultyProfile { Id = facultyId, IdentityUserId = userId, Name = "Prof", Email = "f@v.e" };
        var branch = new Branch { Id = 1, Name = "Dublin" };
        context.Branches.Add(branch);
        context.FacultyProfiles.Add(faculty);
        
        context.Courses.Add(new Course { Name = "My Course", FacultyProfileId = facultyId, BranchId = 1 });
        context.Courses.Add(new Course { Name = "Other Course", FacultyProfileId = "other", BranchId = 1 });
        await context.SaveChangesAsync();

        var user = new IdentityUser { Id = userId, UserName = "faculty@nixuge.me", Email = "faculty@nixuge.me" };
        userManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        var controller = new FacultyController(context, userManager.Object);
        controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { 
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }, "mock")) }
        };

        // Act
        var result = await controller.MyCourses();
        var viewResult = Assert.IsType<ViewResult>(result);
        var courses = Assert.IsAssignableFrom<IEnumerable<Course>>(viewResult.Model);

        // Assert
        Assert.Single(courses);
        Assert.Equal("My Course", courses.First().Name);
    }

    [Fact]
    public async Task Faculty_CannotAccessOtherFacultyCourseDetails() {
        // Arrange
        var context = GetDbContext();
        var userManager = GetUserManagerMock();
        var facultyId = "faculty-1";
        var userId = "user-f1";
        
        var faculty = new FacultyProfile { Id = facultyId, IdentityUserId = userId };
        context.FacultyProfiles.Add(faculty);
        
        var otherCourse = new Course { Id = 10, Name = "Other Course", FacultyProfileId = "other" };
        context.Courses.Add(otherCourse);
        await context.SaveChangesAsync();

        var user = new IdentityUser { Id = userId, UserName = "faculty@nixuge.me" };
        userManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        var controller = new FacultyController(context, userManager.Object);
        controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { 
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }, "mock")) }
        };

        // Act
        var result = await controller.CourseDetails(10);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Student_MyGrades_FiltersByStudentId() {
        // Arrange
        var context = GetDbContext();
        var userManager = GetUserManagerMock();
        var student1Id = "student-1";
        var userId1 = "user-1";
        
        var student1 = new StudentProfile { Id = student1Id, IdentityUserId = userId1 };
        context.StudentProfiles.Add(student1);
        
        var assignment = new Assignment { Title = "A1", MaxScore = 100 };
        context.Assignments.Add(assignment);
        
        context.AssignmentResults.Add(new AssignmentResult { Assignment = assignment, StudentProfileId = student1Id, Score = 80 });
        context.AssignmentResults.Add(new AssignmentResult { Assignment = assignment, StudentProfileId = "student-2", Score = 90 });
        await context.SaveChangesAsync();

        var user = new IdentityUser { Id = userId1, UserName = "student1@nixuge.me" };
        userManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        var controller = new StudentController(context, userManager.Object);
        controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { 
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }, "mock")) }
        };

        // Act
        await controller.MyGrades();
        var assignmentResults = controller.ViewBag.AssignmentResults as IEnumerable<AssignmentResult>;

        // Assert
        Assert.Single(assignmentResults!);
        Assert.Equal(80, assignmentResults!.First().Score);
    }

    [Fact]
    public async Task Faculty_CanUpdateAssignmentResult() {
        // Arrange
        var context = GetDbContext();
        var userManager = GetUserManagerMock();
        var facultyId = "faculty-1";
        var userId = "user-f1";
        
        var faculty = new FacultyProfile { Id = facultyId, IdentityUserId = userId };
        context.FacultyProfiles.Add(faculty);
        
        var course = new Course { Id = 1, Name = "CS", FacultyProfileId = facultyId };
        context.Courses.Add(course);
        
        var assignment = new Assignment { Id = 1, CourseId = 1, Title = "A1" };
        context.Assignments.Add(assignment);
        await context.SaveChangesAsync();

        var user = new IdentityUser { Id = userId, UserName = "faculty@nixuge.me" };
        userManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        var controller = new FacultyController(context, userManager.Object);
        controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { 
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }, "mock")) }
        };

        // Act
        await controller.UpdateAssignmentResult(1, "student-1", 95, "Great job");

        // Assert
        var result = await context.AssignmentResults.FirstOrDefaultAsync(r => r.AssignmentId == 1 && r.StudentProfileId == "student-1");
        Assert.NotNull(result);
        Assert.Equal(95, result.Score);
    }

    [Fact]
    public async Task Admin_CannotEnrolStudentTwiceInSameCourse() {
        // Arrange
        var context = GetDbContext();
        var userManager = GetUserManagerMock();
        var controller = new StudentsController(context, userManager.Object);
        var student = new StudentProfile { Id = "s1", Name = "John Doe" };
        context.StudentProfiles.Add(student);
        var course = new Course { Id = 1, Name = "CS" };
        context.Courses.Add(course);
        context.CourseEnrolments.Add(new CourseEnrolment { StudentProfileId = "s1", CourseId = 1 });
        await context.SaveChangesAsync();

        // Act
        var enrolment = new CourseEnrolment { StudentProfileId = "s1", CourseId = 1 };
        await controller.Enrol(enrolment);

        // Assert
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal(1, await context.CourseEnrolments.CountAsync());
    }
}