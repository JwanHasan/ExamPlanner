using DatabaseConnection.model;

namespace DatabaseConnection.repo;
public interface ICourseRepo
{
    Task<List<Course>> GetAllAsync();
    Task<Course> AddCourseAsync(string SueCode,string Prefix,string name, int Semester,int ects, int PriorityTier);
    Task<Course> UpdateCourseAsync(string SueCode,string Prefix,string name, int Semester,int ects, int PriorityTier);
    Task<Course?> GetCourseByIdAsync(int id );
    Task<Course?> GetCourseBySueCodeAsync(string SueCode );

    Task<bool> DeleteAsync();
    Task SaveChangesAsync();



}