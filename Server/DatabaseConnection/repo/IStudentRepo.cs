using DatabaseConnection.model;

namespace DatabaseConnection.repo;
public interface IStudentRepo
{
    Task<User> GetUserByStudentIdAsync(int id);
    Task<Student> AddStudentAsync(int id,int userId);
    Task<Student?> UpdateStudentAsync(int id, int userId);
    Task<List<StudentExamAssignment>?> GetStudentExamAsssignmentByIdAsync(int id);
    Task<bool> DeleteStudentByIdAsync(int id);
    Task<List<Enrollment>?> GetEnrollmentByStudentIdAsync(int id);

}