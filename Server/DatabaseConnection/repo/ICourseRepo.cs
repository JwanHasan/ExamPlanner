using DatabaseConnection.model;
using DatabaseConnection.dto;

namespace DatabaseConnection.repo;
public interface ICourseRepo
{
    public Task<Course?> GetCourseBySueCodeAsync(string sueCode);
    public Task<Course> AddAsync(CourseDto courseDto );
    public Task<Course?> GetCourseAsyncById(int id );
    public Task<List<Course>> GetAllCourseAsync();
    public Task<Course?> UpdateCourseAsync(int courseId,CourseDto courseDto);
    public Task<Course?> UpdateNotesAsync(int courseId, string? notes);
    public Task<bool> DeleteAsync(int id);

}