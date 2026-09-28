using DatabaseConnection.dto;
using DatabaseConnection.model;

namespace DatabaseConnection.repo;
public interface IEnrollmentRepo
{
    public Task<Enrollment?> GetEnrollment(int studentId, int classId);
    public Task<Enrollment> AddAsync(EnrollmentDto enrollmentDto);
    public Task<List<Enrollment>> GetAllEnrollmentAsync();
    public Task<List<Class>> GetClassesByStudentId(int id);
    public Task<List<Student>> GetStudentsByClassId(int id);

    public Task<bool> DeleteEnrollmentAsync(int studentId, int classId);
    public Task<Enrollment?> UpdateEnrollmentAsync(int studentId, int classId,EnrollmentDto classDto);
}