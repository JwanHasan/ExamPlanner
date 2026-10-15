using DatabaseConnection.model;

namespace DatabaseConnection.repo;
public interface IStudentExamAssignmentRepo
{
    Task<StudentExamAssignment?> GetByIdAsync(int studentId,int examSessionId);

    Task<List<StudentExamAssignment>> GetAllAsync();

    Task<StudentExamAssignment> AddAsync(int studentId,
        int examSessionId,
        bool extraTime);

    Task<StudentExamAssignment?> UpdateAsync(
        int studentId,
        int examSessionId,
        bool extraTime);
    Task<StudentExamAssignment?> UpdateExtratimeAsync(int studentId,int examSessionId, bool extraTime);

    Task<bool> DeleteAsync(int studentId,int examSessionId);

    Task<List<Student>> GetAllStudentByExamSessionId(int examSessionId);
    Task<List<ExamSession>> GetAllExamSessionByStudentId(int studentId);
}