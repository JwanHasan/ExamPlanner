namespace DatabaseConnection.repo;

using DatabaseConnection.model;

public interface IEnrollmentRepo
{
    Task<List<Enrollment>> GetAllAsync();
    Task<Enrollment?> GetEnrollmentsAsync(int studentId, int PlanningElementId);
    Task<Enrollment> AddEnrollmentAsync(int studentId, int PlanningElementId,string hold,int SourceImportId);
    Task<List<Student>> GetStudentsByPlanningIdIdAsync(int PlanningElementId);
    Task<Enrollment?> UpdateEnrollmentAsync(int currentStudentId,int oldPlanningElementId,int newStudentId,int newPlanningElementId,string hold,int SourceImportId);
    Task<bool> DeleteEnrollmentAsync(int studentId,int PlanningElementId);

}