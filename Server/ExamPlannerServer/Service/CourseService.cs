using ExamPlannerServer.Dto;
using ExamPlannerServer.Import;

namespace ExamPlannerServer.Service;

public class CourseService : ICourseService
{
    public Task<List<CourseDto>> GetAllAsync()
    {
        var fakeCourses = new List<CourseDto>
        {
            new()
            {
                Id = 1,
                Name = "Programming 1",
                Semester = 1,
                Ects = 10,
                SueCode = "PRO1"
            },
            new()
            {
                Id = 2,
                Name = "Programming 2",
                Semester = 2,
                Ects = 10,
                SueCode = "PRO2"
            }
        };
        return Task.FromResult(fakeCourses);
    }
    public Task<CourseDto?> UpdateAsync(int id, UpdateCourseDto dto)
    {
        throw new NotImplementedException();
    }

    // public Task<ImportResultDto> ImportAsync(IFormFile file)
    // {
    //     var result = new ImportResultDto { ImportedCount = 0, Errors = new() { "... to be continued" } };
    //     return Task.FromResult(result);
    // }
    //
    // public Task<CourseDto?> UpdateAsync(int id, UpdateCourseDto dto)
    // {
    //     return Task.FromResult<CourseDto?>(null);
    // }
    //
    //
    //parser logic
    private readonly IExcelImportParser _parser;

    public CourseService(IExcelImportParser parser)
    {
        _parser = parser;
    }
    public Task<ImportResultDto> ImportAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var parseResult = _parser.Parse(stream);

        var result = new ImportResultDto
        {
            ImportedCount = parseResult.Rows.Count,
            Errors = parseResult.Errors
        };

        return Task.FromResult(result);
    }
    
}