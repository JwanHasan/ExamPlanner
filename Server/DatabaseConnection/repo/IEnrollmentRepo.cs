namespace DatabaseConnection.repo;

using DatabaseConnection.model;

public interface IEnrollmentRepo
{
    Task<Enrollment> AddEnrollmentAsync(int studentId, int courseId);
    Task<List<Student>> GetStudentsByCourseIdAsync(int courseId);
    Task<List<Student>> GetStudentsBySuecodeAsync(string sueCode);
    Task<List<Student>> GetCoursesByStudentIdAsync(int studentId);
    Task<List<Course>> GetCoursesByVIAIdAsync(int viaId);
    Task<Enrollment> UpdateEnrollmentAsync(int currentStudentId,int currentCourseId,int newStudentId,int newCouseId);
    Task<bool> DeleteEnrollmentAsync(int studentId,int courseId);

}