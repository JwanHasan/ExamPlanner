using Microsoft.AspNetCore.Mvc;
using ExamPlannerServer.Dto;
using ExamPlannerServer.Service;

namespace ExamPlannerServer.controller;

[ApiController]
[Route("api/[controller]")]

public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    // GET /api/courses
    [HttpGet]
    public async Task<IActionResult> GetCourses()
    {
        var courses = await _courseService.GetAllAsync();
        return Ok(courses);
    }

    // POST /api/courses/import
    [HttpPost("import")]
    public async Task<IActionResult> ImportCourses(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var result = await _courseService.ImportAsync(file);
        return Ok(result);
    }

    // PATCH /api/courses/5
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
    {
        var updated = await _courseService.UpdateAsync(id, dto);
        if (updated is null) return NotFound();
        return Ok(updated);
    }
}