using DatabaseConnection.model;
using DatabaseConnection.repo;
using ExamPlannerServer.Dto;

namespace ExamPlannerServer.Service;

public class CourseService : ICourseService
{
    private readonly ICourseRepo _courseRepo;

    public CourseService(ICourseRepo courseRepo)
    {
        _courseRepo = courseRepo;
    }

    public async Task<List<CourseDto>> GetAllAsync()
    {
        var courses = await _courseRepo.GetAllCourseAsync();
        return courses.Select(ToDto).ToList();
    }

    public Task<ImportResultDto> ImportAsync(IFormFile file)
    {
        var result = new ImportResultDto
        {
            ImportedCount = 0,
            Errors = new() { "... to be continued" }
        };

        return Task.FromResult(result);
    }

    public async Task<CourseDto?> UpdateAsync(int id, UpdateCourseDto dto)
    {
        var updatedCourse = await _courseRepo.UpdateNotesAsync(id, dto.Notes);

        return updatedCourse is null
            ? null
            : ToDto(updatedCourse);
    }

    private static CourseDto ToDto(Course course)
    {
        var semesters = course.Classes
            .Select(c => c.Semester)
            .Distinct()
            .OrderBy(semester => semester)
            .ToList();

        return new CourseDto
        {
            Id = course.Id,
            Name = course.Name,
            Semester = string.Join(", ", semesters),
            SueCode = course.SueCode,
            Ects = course.ECTS,
            Notes = course.Notes,
            ClassCount = course.Classes.Count
        };
    }
}