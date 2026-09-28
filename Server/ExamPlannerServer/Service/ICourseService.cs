namespace ExamPlannerServer.Service;
using ExamPlannerServer.Dto;

public interface ICourseService
{
    Task<List<CourseDto>> GetAllAsync();
    Task<ImportResultDto> ImportAsync(IFormFile file);
    Task<CourseDto?> UpdateAsync(int id, UpdateCourseDto dto);
}