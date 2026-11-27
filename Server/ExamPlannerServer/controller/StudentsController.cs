using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using ExamPlannerServer.Auth;
using ExamPlannerServer.Contracts;
using ExamPlannerServer.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExamPlannerServer.controller;

/// <summary>
/// Students (API_CONTRACT.md 8). Teachers and planners can read them; only planners can delete one.
/// A student can only read their own information, through /me: it takes no id, so there is no way to ask
/// for somebody else.
/// </summary>
[ApiController]
[Route("api/v1/students")]
[Authorize]
public class StudentsController(AppDbContext db) : ControllerBase
{
    // GET /api/v1/students
    [HttpGet]
    [Authorize(Roles = ApiRoles.Staff)]
    public async Task<ActionResult<List<StudentDto>>> GetAll() =>
        await db.Student
            .AsNoTracking()
            .OrderBy(s => s.SourceStudentId)
            .Select(s => new StudentDto(s.Id, s.SourceStudentId.ToString(), s.User.Name, s.AttemptNo))
            .ToListAsync();

    // GET /api/v1/students/5
    [HttpGet("{id:int}")]
    [Authorize(Roles = ApiRoles.Staff)]
    public async Task<ActionResult<StudentDto>> Get(int id)
    {
        var student = await db.Student
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StudentDto(s.Id, s.SourceStudentId.ToString(), s.User.Name, s.AttemptNo))
            .SingleOrDefaultAsync();

        return student is null ? NotFound(Unknown($"Student {id} does not exist.")) : student;
    }

    // GET /api/v1/students/me  -> the logged-in student's own information
    [HttpGet("me")]
    [Authorize(Roles = ApiRoles.Student)]
    public async Task<ActionResult<MyStudentDto>> GetMe()
    {
        if (!int.TryParse(User.FindFirst(AuthClaims.UserId)?.Value, out var userId))
            return Unauthorized(new ApiError(ErrorCodes.Unauthorized, "You need to log in."));

        var me = await db.Student
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => new { s.Id, s.SourceStudentId, s.User.Name, s.AttemptNo })
            .FirstOrDefaultAsync();
        if (me is null)
            return NotFound(Unknown("There is no student record for your account."));

        var enrollments = await db.Enrollment
            .AsNoTracking()
            .Where(e => e.StudentId == me.Id)
            .OrderBy(e => e.PlanningEducationElement.StartDate).ThenBy(e => e.PlanningEducationElement.Nickmane)
            .Select(e => new MyEnrollmentDto(
                e.PlanningElementId,
                e.PlanningEducationElement.Name,
                e.PlanningEducationElement.Nickmane,
                e.PlanningEducationElement.Course.SueCode,
                e.PlanningEducationElement.Course.Name,
                e.PlanningEducationElement.Campus.Name,
                e.Hold,
                e.PlanningEducationElement.ECTS,
                e.PlanningEducationElement.StartDate,
                e.PlanningEducationElement.EndDate))
            .ToListAsync();

        return new MyStudentDto(me.Id, me.SourceStudentId.ToString(), me.Name, me.AttemptNo, enrollments);
    }

    // DELETE /api/v1/students/5  (planners only). Their enrollments go with them (database cascade).
    [HttpDelete("{id:int}")]
    [Authorize(Roles = ApiRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var student = await db.Student
            .Where(s => s.Id == id)
            .Select(s => new { s.UserId, s.User.Role })
            .SingleOrDefaultAsync();
        if (student is null)
            return NotFound(Unknown($"Student {id} does not exist."));

        // An account that exists only for this student goes too (the student cascades with it).
        // Any other account is kept, and only the student record is removed.
        if (student.Role == Role.Student)
            await db.User.Where(u => u.Id == student.UserId).ExecuteDeleteAsync();
        else
            await db.Student.Where(s => s.Id == id).ExecuteDeleteAsync();

        return NoContent();
    }

    private static ApiError Unknown(string message) => new(ErrorCodes.StudentNotFound, message);
}
