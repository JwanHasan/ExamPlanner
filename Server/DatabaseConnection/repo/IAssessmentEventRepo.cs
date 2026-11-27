using DatabaseConnection.model;

namespace DatabaseConnection.repo;

public interface IAssessmentEventRepo
{
    Task<AssessmentEvent?> GetByIdAsync (int id );
    Task<AssessmentEvent?> GetEventByPlanningElementIdAsync (Guid planningEducationElementId );
    Task<List<AssessmentEvent>> GetAllAsync();
    Task<PlanningEducationElement?> GetEducationElementAsync(int id);
    Task<ExamType?> GetExamTypeByIdAsync(int id);
    Task<string> GetNoteById(int id);
    Task<AssessmentEvent?> UpdateExamTypeById(int id, ExamType type);
    Task<AssessmentEvent?> UpdateNoteById(int id, string note);
    Task<AssessmentEvent?> UpdateByIdAsync(int id,
        Guid planningEducationElementId, ExamType examType, string note,int RequiredExamDays );
    Task<bool> DeleteByIdAsync(int id);

}
