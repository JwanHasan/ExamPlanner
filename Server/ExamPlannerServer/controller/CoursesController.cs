using DatabaseConnection.model;
using DatabaseConnection.repo;
using ExamPlannerServer.Auth;
using ExamPlannerServer.Contracts;
using ExamPlannerServer.Dto;
using ExamPlannerServer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamPlannerServer.controller;

/// <summary>
/// Courses from the database, through the teammates' CourseRepo and ClassRepo.
/// Not part of API_CONTRACT.md yet; it uses the contract's /api/v1 base path and error shape.
/// </summary>
[ApiController]
[Route("api/v1/courses")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseRepo _courses;
    private readonly IClassRepo _classes;
    private readonly ICourseService _courseService;

    public CoursesController(ICourseRepo courses, IClassRepo classes, ICourseService courseService)
    {
        _courses = courses;
        _classes = classes;
        _courseService = courseService;
    }

    // GET /api/v1/courses
    [HttpGet]
    public async Task<ActionResult<List<CourseDto>>> GetCourses()
    {
        var courses = await _courses.GetAllAsync();
        var classCounts = await ClassCountsAsync();

        return courses
            .OrderBy(c => c.Prefix).ThenBy(c => c.SueCode)
            .Select(c => ToDto(c, classCounts.GetValueOrDefault(c.Id)))
            .ToList();
    }

    // GET /api/v1/courses/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CourseDto>> GetCourse(int id)
    {
        var course = await _courses.GetCourseByIdAsync(id);
        if (course is null)
            return NotFound(new ApiError(ErrorCodes.CourseNotFound, $"Course {id} does not exist."));

        return ToDto(course, await ClassCountAsync(id));
    }

    // PATCH /api/v1/courses/5  (only the exam planners change data)
    // Name and SueCode are saved. Notes and ClassCount have no column in the database, so they are ignored.
    [HttpPatch("{id:int}")]
    [Authorize(Roles = AdminSettings.AdminRole)]
    public async Task<ActionResult<CourseDto>> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.SueCode))
            return BadRequest(new ApiError(ErrorCodes.InvalidRequest, "Name and SueCode must not be empty."));

        var course = await _courses.GetCourseByIdAsync(id);
        if (course is null)
            return NotFound(new ApiError(ErrorCodes.CourseNotFound, $"Course {id} does not exist."));

        var updated = await _courses.UpdateCourseByIdAsync(id, dto.SueCode.Trim(), course.Prefix, dto.Name.Trim(),
            course.Semester, course.ECTS, course.PriorityTier);

        // Counted after saving, so no classes are loaded while the course is being saved.
        return ToDto(updated, await ClassCountAsync(id));
    }

    // POST /api/v1/courses/import  -> only reads the planning-element export and reports
    // how many rows it understood. It saves nothing; the real import is POST /api/v1/imports.
    [HttpPost("import")]
    public async Task<IActionResult> ImportCourses(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ApiError(ErrorCodes.FileRequired, "Choose an Excel file."));

        var result = await _courseService.ImportAsync(file);
        return Ok(result);
    }

    private async Task<Dictionary<int, int>> ClassCountsAsync() =>
        (await _classes.GetClassesAsync())
            .GroupBy(c => c.CourseId)
            .ToDictionary(g => g.Key, g => g.Count());

    private async Task<int> ClassCountAsync(int courseId) =>
        (await _classes.GetClassesByCourseIdAsync(courseId)).Count;

    private static CourseDto ToDto(Course course, int classCount) => new()
    {
        Id = course.Id,
        Name = course.Name,
        Semester = course.Semester,
        SueCode = course.SueCode,
        Ects = course.ECTS,
        Notes = null,
        ClassCount = classCount
    };
}
