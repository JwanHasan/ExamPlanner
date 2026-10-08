using DatabaseConnection.model;

namespace DatabaseConnection.repo;
public interface ICourseRepo
{
    Task<List<Course>> GetAllAsync();
    Task<Course> AddCourseAsync(string SueCode,string Prefix,string name, int Semester,int ects, int PriorityTier);
    Task<Course> UpdateCourseByIdAsync(int id,string SueCode,string Prefix,string name, int Semester,int ects, int PriorityTier);
    Task<Course> UpdateCourseBySuecodeAsync(string SueCode,string Prefix,string name, int Semester,int ects, int PriorityTier);

    Task<Course?> GetCourseByIdAsync(int id );
    Task<Course?> GetCourseBySueCodeAsync(string SueCode );

    Task<bool> DeleteAsync(int id);
    Task SaveChangesAsync();



}