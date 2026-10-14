using DatabaseConnection.model;

namespace DatabaseConnection.repo;

public interface IAssessmentEventRepo
{
    Task<AssessmentEvent?> GetByIdAsync (int id );
    Task<AssessmentEvent?> GetEventByPlanningElementIdAsync (Guid planningEducationElementId );
    Task<List<AssessmentEvent>> GetAllAsync();
    Task<List<AssessmentEvent>> GetAllByCourseIdAsync(int id);
    Task<Course?> GetCourseAsync(int id);
    Task<PlanningEducationElement> GetEducationElementAsync(int id);
    Task<ExamType?> GetExamTypeByIdAsync(int id);
    Task<string> GetNoteById(int id);
    Task<AssessmentEvent?> UpdateExamTypeById(int id, ExamType type);
    Task<AssessmentEvent?> UpdateNoteById(int id, string note);
    Task<AssessmentEvent?> UpdateByIdAsync(int id,
        int courseId,int planningEducationElementId, ExamType examType, string note );
    Task<bool> DeleteByIdAsync(int id);

}
