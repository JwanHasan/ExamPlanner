namespace DatabaseConnection.repo;

using DatabaseConnection.model;

public interface IEnrollmentRepo
{
    Task<List<Enrollment>> GetAllAsync();
    Task<Enrollment?> GetEnrollmentsAsync(int studentId, Guid PlanningElementId);
    Task<Enrollment> AddEnrollmentAsync(int studentId, Guid PlanningElementId,string hold,int SourceImportId);
    Task<List<Student>> GetStudentsByPlanningIdIdAsync(Guid PlanningElementId);
    Task<Enrollment?> UpdateEnrollmentAsync(int currentStudentId,Guid oldPlanningElementId,int newStudentId,Guid newPlanningElementId,string hold,int SourceImportId);
    Task<bool> DeleteEnrollmentAsync(int studentId,Guid PlanningElementId);

}