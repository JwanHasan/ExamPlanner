using DatabaseConnection.DBContext;
using DatabaseConnection.model;

namespace DatabaseConnection.repo;

public class EnrollmentRepo : IEnrollmentRepo
{
    private readonly AppDbContext _context;
    public EnrollmentRepo(AppDbContext context)
    {
        _context=context;
    }

    public Task<Enrollment> AddEnrollmentAsync(int studentId, int courseId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteEnrollmentAsync(int studentId, int courseId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Student>> GetCoursesByStudentIdAsync(int studentId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Course>> GetCoursesByVIAIdAsync(int viaId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Student>> GetStudentsByCourseIdAsync(int courseId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Student>> GetStudentsBySuecodeAsync(string sueCode)
    {
        throw new NotImplementedException();
    }

    public Task<Enrollment> UpdateEnrollmentAsync(int currentStudentId, int currentCourseId, int newStudentId, int newCouseId)
    {
        throw new NotImplementedException();
    }
}